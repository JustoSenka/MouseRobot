using RobotRuntime.Abstractions;
using RobotRuntime.Assets;
using RobotRuntime.Execution;
using RobotRuntime.Graphics;
using RobotRuntime.Settings;
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
        public TestRunner(IAssetGuidManager AssetGuidManager, IScreenStateThread ScreenStateThread, IFeatureDetectionThread FeatureDetectionThread,
            IRunnerFactory RunnerFactory, IPluginLoader PluginLoader)
        {
            this.AssetGuidManager = AssetGuidManager;
            this.ScreenStateThread = ScreenStateThread;
            this.FeatureDetectionThread = FeatureDetectionThread;
            this.RunnerFactory = RunnerFactory;
            this.PluginLoader = PluginLoader;
        }

        public void Start(string projectPath, string scriptName)
        {
            PluginLoader.UserAssemblyName = "CustomAssembly.dll";
            PluginLoader.UserAssemblyPath = Path.Combine(Paths.MetadataPath, PluginLoader.UserAssemblyName);
            PluginLoader.CreateUserAppDomain();

            //RuntimeSettings.ApplySettings(); // Need to get some sort of settings (appdata has them)

            var importer = AssetImporter.FromPath(Path.Combine(projectPath, scriptName) + FileExtensions.ScriptD);
            Start(importer.Load<LightScript>());
        }

        public void Start(LightScript lightScript)
        {
            RunnerFactory.Callback = RunningCommand;

            ShouldCancelRun.Value = false;
            RunnerFactory.CancellingPointerPlaceholder = ShouldCancelRun;

            AssetGuidManager.LoadMetaFiles();

            //RuntimeSettings.ApplySettings();
            ScreenStateThread.Start();
            FeatureDetectionThread.Start();
            Task.Delay(80).Wait(); // make sure first screenshot is taken before starting running commands


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
