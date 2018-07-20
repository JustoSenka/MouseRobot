using RobotRuntime.Abstractions;
using RobotRuntime.Execution;
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
        public event CommandRunningCallback RunningCommandCallback;
        private ValueWrapper<bool> ShouldCancelRun = new ValueWrapper<bool>(false);

        public event Action TestRunStart;
        public event Action TestRunEnd;

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
            ShouldCancelRun.Value = false;

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

            RunnerFactory.PassDependencies(lightScript, RunningCommandCallback, ShouldCancelRun);
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
                    RunScriptIfNotEmpty(cachedScriptRunner, fixture.OneTimeSetup);
                    if (ShouldCancelRun.Value) return;

                    var fixtureMathesFilter = Regex.IsMatch(fixture.Name, testFilter);

                    foreach (var test in fixture.Tests)
                    {
                        var testMathesFilter = Regex.IsMatch(test.Name, testFilter);
                        if (!fixtureMathesFilter && !testMathesFilter)
                            continue;

                        RunScriptIfNotEmpty(cachedScriptRunner, fixture.Setup);
                        if (ShouldCancelRun.Value) return;

                        RunnerFactory.PassDependencies(test, RunningCommandCallback, ShouldCancelRun);
                        cachedScriptRunner.Run(test);
                        if (ShouldCancelRun.Value) return;

                        RunScriptIfNotEmpty(cachedScriptRunner, fixture.TearDown);
                        if (ShouldCancelRun.Value) return;
                    }

                    RunScriptIfNotEmpty(cachedScriptRunner, fixture.OneTimeTeardown);
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
                RunnerFactory.PassDependencies(script, RunningCommandCallback, ShouldCancelRun);
                scriptRunner.Run(script);
            }
        }

        public void Stop()
        {
            ShouldCancelRun.Value = true;
        }
    }
}
