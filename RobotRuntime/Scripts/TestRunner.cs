using RobotRuntime.Abstractions;
using RobotRuntime.Execution;
using RobotRuntime.Scripts;
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
        public TestData TestData { get; } = new TestData()
        {
            ShouldCancelRun = false
        };

        public event Action TestRunStart;
        public event Action TestRunEnd;

        public event Action<LightTestFixture, Script> FixtureSpecialScripFailed;
        public event Action<LightTestFixture, Script> FixtureSpecialScriptSucceded;
        public event Action<LightTestFixture, Script> TestPassed;
        public event Action<LightTestFixture, Script> TestFailed;

        private IAssetGuidManager AssetGuidManager;
        private IScreenStateThread ScreenStateThread;
        private IFeatureDetectionThread FeatureDetectionThread;
        private IRunnerFactory RunnerFactory;
        private IPluginLoader PluginLoader;
        private IRuntimeSettings RuntimeSettings;
        private IRuntimeAssetManager RuntimeAssetManager;
        public TestRunner(IAssetGuidManager AssetGuidManager, IScreenStateThread ScreenStateThread, IFeatureDetectionThread FeatureDetectionThread,
            IRunnerFactory RunnerFactory, IPluginLoader PluginLoader, IRuntimeSettings RuntimeSettings, IRuntimeAssetManager RuntimeAssetManager)
        {
            this.AssetGuidManager = AssetGuidManager;
            this.ScreenStateThread = ScreenStateThread;
            this.FeatureDetectionThread = FeatureDetectionThread;
            this.RunnerFactory = RunnerFactory;
            this.PluginLoader = PluginLoader;
            this.RuntimeSettings = RuntimeSettings;
            this.RuntimeAssetManager = RuntimeAssetManager;
        }

        private void InitializeProject(string projectPath)
        {
            Environment.CurrentDirectory = projectPath;

            PluginLoader.UserAssemblyName = "CustomAssembly.dll";
            PluginLoader.UserAssemblyPath = Path.Combine(Paths.MetadataPath, PluginLoader.UserAssemblyName);
            PluginLoader.CreateUserAppDomain();

            RuntimeSettings.LoadSettingsHardcoded();
        }

        private void InitializeNewRun()
        {
            TestRunStart?.Invoke();
            TestData.ShouldCancelRun = false;

            AssetGuidManager.LoadMetaFiles();
            RuntimeAssetManager.CollectAllImporters();

            if (!ScreenStateThread.IsAlive)
                ScreenStateThread.Start();

            if (!FeatureDetectionThread.IsAlive)
                FeatureDetectionThread.Start();
        }

        /// <summary>
        /// This method should solely be used when running from command line
        /// </summary>
        public void StartScript(string projectPath, string scriptName)
        {
            InitializeProject(projectPath);

            var importer = AssetImporter.FromPath(Path.Combine(Paths.ScriptPath, scriptName) + FileExtensions.ScriptD);
            StartScript(importer.Load<LightScript>());
        }

        /// <summary>
        /// This method should solely be used when running from command line
        /// </summary>
        public void StartTests(string projectPath, string testFilter = "")
        {
            InitializeProject(projectPath);
            StartTests(testFilter);
        }

        public void StartScript(LightScript lightScript)
        {
            InitializeNewRun();

            TestData.TestFixture = lightScript;
            RunnerFactory.PassDependencies(TestData);

            Task.Delay(150).Wait(); // make sure first screenshot is taken before starting running commands

            new Thread(delegate ()
            {
                var runner = RunnerFactory.GetFor(lightScript.GetType());
                runner.Run(lightScript);

                ScreenStateThread.Stop();
                FeatureDetectionThread.Stop();

                TestRunEnd?.Invoke();
            }).Start();
        }

        /// <summary>
        /// Runs all tests from all test fixtures.
        /// Accepts Regex filter for fixture and test names.
        /// </summary>
        public void StartTests(string testFilter = ".")
        {
            InitializeNewRun();

            new Thread(delegate ()
            {
                var fixtureImporters = RuntimeAssetManager.AssetImporters.Where(importer => importer.HoldsType() == typeof(LightTestFixture));
                var fixtures = fixtureImporters.Select(i => i.Load<LightTestFixture>()).Where(value => value != null); // If test fixuture failed to import, it might be null. Ignore them

                // RunnerFactory.GetFor uses reflection to check attribute, might be faster to just cache the value
                var cachedScriptRunner = RunnerFactory.GetFor(typeof(LightScript));

                foreach (var fixture in fixtures)
                {
                    var fixtureMathesFilter = Regex.IsMatch(fixture.Name, testFilter);

                    var isThereASingleTestMatchingFilter = fixture.Tests.Any(test => Regex.IsMatch(test.Name, testFilter));
                    if (!isThereASingleTestMatchingFilter && !fixtureMathesFilter)
                        continue;

                    RunScriptIfNotEmpty(cachedScriptRunner, fixture.OneTimeSetup);
                    if (TestData.ShouldCancelRun) return;
                    CheckIfTestFailedAndFireCallbacks(fixture, fixture.OneTimeSetup);

                    foreach (var test in fixture.Tests)
                    {
                        var testMathesFilter = Regex.IsMatch(test.Name, testFilter);
                        if (!fixtureMathesFilter && !testMathesFilter)
                            continue;

                        // Setup
                        RunScriptIfNotEmpty(cachedScriptRunner, fixture.Setup);
                        if (TestData.ShouldCancelRun) return;
                        CheckIfTestFailedAndFireCallbacks(fixture, fixture.Setup);

                        // Test 
                        RunnerFactory.PassDependencies(TestData);
                        cachedScriptRunner.Run(test);
                        if (TestData.ShouldCancelRun) return;
                        CheckIfTestFailedAndFireCallbacks(fixture, test);

                        // Teardown
                        RunScriptIfNotEmpty(cachedScriptRunner, fixture.TearDown);
                        if (TestData.ShouldCancelRun) return;
                        CheckIfTestFailedAndFireCallbacks(fixture, fixture.TearDown);
                    }

                    RunScriptIfNotEmpty(cachedScriptRunner, fixture.OneTimeTeardown);
                    CheckIfTestFailedAndFireCallbacks(fixture, fixture.OneTimeTeardown);
                }

                ScreenStateThread.Stop();
                FeatureDetectionThread.Stop();

                TestRunEnd?.Invoke();
            }).Start();
        }

        private void RunScriptIfNotEmpty(IRunner scriptRunner, LightScript script)
        {
            if (script.Commands.Count() > 0)
            {
                RunnerFactory.PassDependencies(TestData);
                scriptRunner.Run(script);
            }
        }

        private bool CheckIfTestFailedAndFireCallbacks(LightTestFixture fixture, Script script)
        {
            var shouldFailTest = TestData.ShouldFailTest;

            if (LightTestFixture.IsSpecialScript(script) && shouldFailTest)
                FixtureSpecialScripFailed?.Invoke(fixture, script);

            else if (LightTestFixture.IsSpecialScript(script) && !shouldFailTest)
                FixtureSpecialScriptSucceded?.Invoke(fixture, script);

            else if (shouldFailTest)
                TestFailed?.Invoke(fixture, script);

            else
                TestPassed?.Invoke(fixture, script);

            TestData.ShouldFailTest = false;
            return shouldFailTest;
        }

        public void Stop()
        {
            TestData.ShouldCancelRun = true;
        }
    }
}
