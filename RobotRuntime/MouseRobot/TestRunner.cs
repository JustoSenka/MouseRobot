using RobotRuntime.Abstractions;
using RobotRuntime.Execution;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private readonly IRunnerFactory RunnerFactory;
        private readonly IScriptLoader ScriptLoader;
        private readonly IRuntimeAssetManager RuntimeAssetManager;
        private readonly IRuntimeSettings RuntimeSettings;
        public TestRunner(IAssetGuidManager AssetGuidManager, IRunnerFactory RunnerFactory, IScriptLoader ScriptLoader, 
            IRuntimeAssetManager RuntimeAssetManager, IRuntimeSettings RuntimeSettings)
        {
            this.AssetGuidManager = AssetGuidManager;
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
            ResetTestData(TestData);

            AssetGuidManager.LoadMetaFiles();
            RuntimeAssetManager.CollectAllImporters();
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

            ResetTestData(TestData);
            TestData.TestFixture = lightRecording;
            RunnerFactory.PassDependencies(TestData);

            return Task.Run(() =>
            {
                Task.Delay(150).Wait(); // make sure first screenshot is taken before starting running commands

                var runner = RunnerFactory.GetFor(lightRecording.GetType());
                runner.Run(lightRecording);

                TestRunEnd?.Invoke();
            });
        }

        private void ResetTestData(TestData testData)
        {
            testData.ShouldPassTest = false;
            testData.ShouldFailTest = false;
            testData.ShouldCancelRun = false;
        }

        /// <summary>
        /// Runs all tests from all test fixtures.
        /// Accepts Regex filter for fixture and test names.
        /// </summary>
        public Task StartTests(string testFilter = ".")
        {
            if (string.IsNullOrEmpty(testFilter))
                testFilter = ".";

            InitializeNewRun();

            return StartTestsInternal(testFilter);
        }

        private async Task StartTestsInternal(string testFilter)
        {
            await Task.Run(() =>
            {
                var fixtureImporters = RuntimeAssetManager.AssetImporters.Where(importer => importer.HoldsType() == typeof(LightTestFixture));
                var fixtures = fixtureImporters.Select(i => i.Load<LightTestFixture>()).Where(value => value != null); // If test fixuture failed to import, it might be null. Ignore them

                // RunnerFactory.GetFor uses reflection to check attribute, might be faster to just cache the value
                var cachedRecordingRunner = RunnerFactory.GetFor(typeof(LightRecording));

                foreach (var fixture in fixtures)
                {
                    var fixtureMathesFilter = Regex.IsMatch(fixture.Name, testFilter, RegexOptions.IgnoreCase);

                    var isThereASingleTestMatchingFilter = fixture.Tests.Any(test => Regex.IsMatch(fixture.Name + "." + test.Name, testFilter, RegexOptions.IgnoreCase));
                    if (!isThereASingleTestMatchingFilter && !fixtureMathesFilter)
                        continue;

                    FixtureIsBeingRun?.Invoke(fixture);

                    RunRecordingIfNotEmpty(cachedRecordingRunner, fixture.OneTimeSetup);
                    if (FireCallbacksAndCancelRunIfNeeded()) return;
                    CheckIfTestFailedAndFireCallbacks(fixture, fixture.OneTimeSetup);

                    foreach (var test in fixture.Tests)
                    {
                        ResetTestData(TestData);

                        var testMathesFilter = Regex.IsMatch(fixture.Name + "." + test.Name, testFilter, RegexOptions.IgnoreCase);
                        if (!fixtureMathesFilter && !testMathesFilter)
                            continue;

                        TestIsBeingRun?.Invoke(fixture, test);

                        // Setup
                        RunRecordingIfNotEmpty(cachedRecordingRunner, fixture.Setup);
                        if (FireCallbacksAndCancelRunIfNeeded()) return;
                        CheckIfTestFailedAndFireCallbacks(fixture, fixture.Setup);

                        // Test 
                        RunnerFactory.PassDependencies(TestData);
                        cachedRecordingRunner.Run(test);
                        if (FireCallbacksAndCancelRunIfNeeded()) return;
                        CheckIfTestFailedAndFireCallbacks(fixture, test);

                        // Teardown
                        RunRecordingIfNotEmpty(cachedRecordingRunner, fixture.TearDown);
                        if (FireCallbacksAndCancelRunIfNeeded()) return;
                        CheckIfTestFailedAndFireCallbacks(fixture, fixture.TearDown);
                    }

                    RunRecordingIfNotEmpty(cachedRecordingRunner, fixture.OneTimeTeardown);
                    CheckIfTestFailedAndFireCallbacks(fixture, fixture.OneTimeTeardown);
                }

                TestRunEnd?.Invoke();
            });
        }

        private bool FireCallbacksAndCancelRunIfNeeded()
        {
            if (TestData.ShouldCancelRun)
                TestRunEnd?.Invoke();

            return TestData.ShouldCancelRun;
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
