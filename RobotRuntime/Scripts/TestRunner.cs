using RobotRuntime.Abstractions;
using RobotRuntime.Assets;
using RobotRuntime.Execution;
using RobotRuntime.Graphics;
using RobotRuntime.Utils;
using System;
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
        public TestRunner(IAssetGuidManager AssetGuidManager, IScreenStateThread ScreenStateThread, IFeatureDetectionThread FeatureDetectionThread, IRunnerFactory RunnerFactory)
        {
            this.AssetGuidManager = AssetGuidManager;
            this.ScreenStateThread = ScreenStateThread;
            this.FeatureDetectionThread = FeatureDetectionThread;
            this.RunnerFactory = RunnerFactory;
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
