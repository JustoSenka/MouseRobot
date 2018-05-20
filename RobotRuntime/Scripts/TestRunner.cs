using RobotRuntime.Abstractions;
using RobotRuntime.Execution;
using RobotRuntime.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RobotRuntime
{
    public class TestRunner : ITestRunner
    {
        public event Action Finished;
        public event CommandRunningCallback RunningCommand;

        private ValueWrapper<bool> ShouldCancelRun = new ValueWrapper<bool>(false);

        private IAssetGuidManager AssetGuidManager;
        private IScreenStateThread ScreenStateThread;
        private IFeatureDetectionThread FeatureDetectionThread;
        private IRunnerFactory RunnerFactory;
        private IPluginLoader PluginLoader;
        private IRuntimeSettings RuntimeSettings;
        public TestRunner(IAssetGuidManager AssetGuidManager, IScreenStateThread ScreenStateThread, IFeatureDetectionThread FeatureDetectionThread,
            IRunnerFactory RunnerFactory, IPluginLoader PluginLoader, IRuntimeSettings RuntimeSettings)
        {
            this.AssetGuidManager = AssetGuidManager;
            this.ScreenStateThread = ScreenStateThread;
            this.FeatureDetectionThread = FeatureDetectionThread;
            this.RunnerFactory = RunnerFactory;
            this.PluginLoader = PluginLoader;
            this.RuntimeSettings = RuntimeSettings;
        }

        public void Start(string projectPath, string scriptName)
        {
            PluginLoader.UserAssemblyName = "CustomAssembly.dll";
            PluginLoader.UserAssemblyPath = Path.Combine(Paths.MetadataPath, PluginLoader.UserAssemblyName);
            PluginLoader.CreateUserAppDomain();

            RuntimeSettings.LoadSettingsHardcoded();

            var importer = AssetImporter.FromPath(Path.Combine(Paths.ScriptPath, scriptName) + FileExtensions.ScriptD);
            Start(importer.Load<LightScript>());
        }

        public void Start(LightScript lightScript)
        {
            RunnerFactory.Callback = RunningCommand;

            ShouldCancelRun.Value = false;
            RunnerFactory.CancellingPointerPlaceholder = ShouldCancelRun;

            AssetGuidManager.LoadMetaFiles();

            if (!ScreenStateThread.IsAlive)
                ScreenStateThread.Start();

            if (!FeatureDetectionThread.IsAlive)
                FeatureDetectionThread.Start();

            Task.Delay(150).Wait(); // make sure first screenshot is taken before starting running commands

            new Thread(delegate ()
            {
                Logger.Log(LogType.Debug, "Script start");

                var runner = RunnerFactory.CreateFor(lightScript.GetType());
                runner.Run(lightScript);

                ScreenStateThread.Stop();
                FeatureDetectionThread.Stop();

                Finished?.Invoke();
                Logger.Log(LogType.Debug, "Script end");
            }).Start();
        }

        public void Stop()
        {
            ShouldCancelRun.Value = true;
        }
    }
}
