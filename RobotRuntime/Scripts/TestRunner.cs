using RobotRuntime.Abstractions;
using RobotRuntime.Execution;
using RobotRuntime.Tests;
using RobotRuntime.Utils;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RobotRuntime
{
    public class TestRunner : ITestRunner
    {
        public event Action Finished;
        public event CommandRunningCallback RunningCommandCallback;

        private ValueWrapper<bool> ShouldCancelRun = new ValueWrapper<bool>(false);

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
                Logger.Log(LogType.Debug, "Script start");

                var runner = RunnerFactory.GetFor(lightScript.GetType());
                runner.Run(lightScript);

                ScreenStateThread.Stop();
                FeatureDetectionThread.Stop();

                Finished?.Invoke();
                Logger.Log(LogType.Debug, "Script end");
            }).Start();
        }

        public void StartTests(string testFilter = "") // TODO: TestFilter
        {
            InitializeNewRun();

            var fixtureImporters = RuntimeAssetManager.AssetImporters.Where(importer => importer.HoldsType() == typeof(LightTestFixture));
            var fixtures = fixtureImporters.Select(i => i.Load<LightTestFixture>()).Where(value => value != null); // If test fixuture failed to import, it might be null. Ignore them

            var cachedScriptRunner = RunnerFactory.GetFor(typeof(LightScript));

            foreach (var fixture in fixtures)
            {
                RunScriptIfNotEmpty(cachedScriptRunner, fixture.OneTimeSetup);

                foreach (var test in fixture.Tests)
                {
                    RunScriptIfNotEmpty(cachedScriptRunner, fixture.Setup);

                    RunnerFactory.PassDependencies(test, RunningCommandCallback, ShouldCancelRun);
                    cachedScriptRunner.Run(test);

                    RunScriptIfNotEmpty(cachedScriptRunner, fixture.TearDown);
                }

                RunScriptIfNotEmpty(cachedScriptRunner, fixture.OneTimeTeardown);
            }
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
