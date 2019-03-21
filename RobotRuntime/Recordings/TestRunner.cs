using RobotRuntime.Abstractions;
using RobotRuntime.Execution;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using RobotRuntime.Utils;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RobotRuntime
{
    public class TestRunner : ITestRunner
    {
        public TestData TestData { get; private set; } = new TestData()
        {
            ShouldCancelRun = false
        };

        public event Action TestRunStart;
        public event Action TestRunEnd;

        public event Action<LightTestFixture> FixtureIsBeingRun;
        public event Action<LightTestFixture, Recording> TestIsBeingRun;

        public event Action<LightTestFixture, Recording> FixtureSpecialRecordingFailed;
        public event Action<LightTestFixture, Recording> FixtureSpecialRecordingSucceded;
        public event Action<LightTestFixture, Recording> TestPassed;
        public event Action<LightTestFixture, Recording> TestFailed;

        private readonly IAssetGuidManager AssetGuidManager;
        private readonly IScreenStateThread ScreenStateThread;
        private readonly IFeatureDetectionThread FeatureDetectionThread;
        private readonly IRunnerFactory RunnerFactory;
        private readonly IScriptLoader ScriptLoader;
        private readonly IRuntimeAssetManager RuntimeAssetManager;
        private readonly IRuntimeSettings RuntimeSettings;
        public TestRunner(IAssetGuidManager AssetGuidManager, IScreenStateThread ScreenStateThread, IFeatureDetectionThread FeatureDetectionThread,
            IRunnerFactory RunnerFactory, IScriptLoader ScriptLoader, IRuntimeAssetManager RuntimeAssetManager, IRuntimeSettings RuntimeSettings)
        {
            this.AssetGuidManager = AssetGuidManager;
            this.ScreenStateThread = ScreenStateThread;
            this.FeatureDetectionThread = FeatureDetectionThread;
            this.RunnerFactory = RunnerFactory;
            this.ScriptLoader = ScriptLoader;
            this.RuntimeAssetManager = RuntimeAssetManager;
            this.RuntimeSettings = RuntimeSettings;
        }

        /// <summary>
        /// This Method should only be called from command line. Editor sets settings on its own
        /// </summary>
        public void LoadSettings()
        {
            RuntimeSettings.LoadSettingsHardcoded();
        }

        private void InitializeNewRun()
        {
            TestRunStart?.Invoke();
            TestData.ShouldCancelRun = false;
            TestData.ShouldFailTest = false;

            AssetGuidManager.LoadMetaFiles();
            RuntimeAssetManager.CollectAllImporters();

            if (!ScreenStateThread.IsAlive)
                ScreenStateThread.Start();

            if (!FeatureDetectionThread.IsAlive)
                FeatureDetectionThread.Start();
        }

        /// <summary>
        /// This method should solely be used when running from command line.
        /// Starts a recording by path. Works with or without extension.
        /// </summary>
        public Task StartRecording(string recordingName)
        {
            // TODO: use RuntimeAssetManager here
            recordingName = Path.HasExtension(recordingName) ? recordingName : recordingName + FileExtensions.RecordingD;
            var importer = AssetImporter.FromPath(recordingName);
            return StartRecording(importer.Load<LightRecording>());
        }

        public Task StartRecording(LightRecording lightRecording)
        {
            InitializeNewRun();

            TestData.TestFixture = lightRecording;
            RunnerFactory.PassDependencies(TestData);

            return Task.Run(() =>
            {
                Task.Delay(150).Wait(); // make sure first screenshot is taken before starting running commands

                var runner = RunnerFactory.GetFor(lightRecording.GetType());
                runner.Run(lightRecording);

                ScreenStateThread.Stop();
                FeatureDetectionThread.Stop();

                TestRunEnd?.Invoke();
            });
        }

        /// <summary>
        /// Runs all tests from all test fixtures.
        /// Accepts Regex filter for fixture and test names.
        /// </summary>
        public Task StartTests(string testFilter = ".")
        {
            InitializeNewRun();

            return Task.Run(() =>
            {
                Task.Delay(150).Wait(); // make sure first screenshot is taken before starting running commands

                var fixtureImporters = RuntimeAssetManager.AssetImporters.Where(importer => importer.HoldsType() == typeof(LightTestFixture));
                var fixtures = fixtureImporters.Select(i => i.Load<LightTestFixture>()).Where(value => value != null); // If test fixuture failed to import, it might be null. Ignore them

                // RunnerFactory.GetFor uses reflection to check attribute, might be faster to just cache the value
                var cachedRecordingRunner = RunnerFactory.GetFor(typeof(LightRecording));

                foreach (var fixture in fixtures)
                {
                    var fixtureMathesFilter = Regex.IsMatch(fixture.Name, testFilter);

                    var isThereASingleTestMatchingFilter = fixture.Tests.Any(test => Regex.IsMatch(fixture.Name + "." + test.Name, testFilter));
                    if (!isThereASingleTestMatchingFilter && !fixtureMathesFilter)
                        continue;

                    FixtureIsBeingRun?.Invoke(fixture);

                    RunRecordingIfNotEmpty(cachedRecordingRunner, fixture.OneTimeSetup);
                    if (TestData.ShouldCancelRun) return;
                    CheckIfTestFailedAndFireCallbacks(fixture, fixture.OneTimeSetup);

                    foreach (var test in fixture.Tests)
                    {
                        var testMathesFilter = Regex.IsMatch(fixture.Name + "." + test.Name, testFilter);
                        if (!fixtureMathesFilter && !testMathesFilter)
                            continue;

                        TestIsBeingRun?.Invoke(fixture, test);

                        // Setup
                        RunRecordingIfNotEmpty(cachedRecordingRunner, fixture.Setup);
                        if (TestData.ShouldCancelRun) return;
                        CheckIfTestFailedAndFireCallbacks(fixture, fixture.Setup);

                        // Test 
                        RunnerFactory.PassDependencies(TestData);
                        cachedRecordingRunner.Run(test);
                        if (TestData.ShouldCancelRun) return;
                        CheckIfTestFailedAndFireCallbacks(fixture, test);

                        // Teardown
                        RunRecordingIfNotEmpty(cachedRecordingRunner, fixture.TearDown);
                        if (TestData.ShouldCancelRun) return;
                        CheckIfTestFailedAndFireCallbacks(fixture, fixture.TearDown);
                    }

                    RunRecordingIfNotEmpty(cachedRecordingRunner, fixture.OneTimeTeardown);
                    CheckIfTestFailedAndFireCallbacks(fixture, fixture.OneTimeTeardown);
                }

                ScreenStateThread.Stop();
                FeatureDetectionThread.Stop();

                TestRunEnd?.Invoke();
            });
        }

        private void RunRecordingIfNotEmpty(IRunner recordingRunner, LightRecording recording)
        {
            if (recording.Commands.Count() > 0)
            {
                RunnerFactory.PassDependencies(TestData);
                recordingRunner.Run(recording);
            }
        }

        private bool CheckIfTestFailedAndFireCallbacks(LightTestFixture fixture, Recording recording)
        {
            var shouldFailTest = TestData.ShouldFailTest;

            if (LightTestFixture.IsSpecialRecording(recording) && shouldFailTest)
                FixtureSpecialRecordingFailed?.Invoke(fixture, recording);

            else if (LightTestFixture.IsSpecialRecording(recording) && !shouldFailTest)
                FixtureSpecialRecordingSucceded?.Invoke(fixture, recording);

            else if (shouldFailTest)
                TestFailed?.Invoke(fixture, recording);

            else
                TestPassed?.Invoke(fixture, recording);

            TestData.ShouldFailTest = false;
            return shouldFailTest;
        }

        public void Stop()
        {
            TestData.ShouldCancelRun = true;
        }
    }
}
