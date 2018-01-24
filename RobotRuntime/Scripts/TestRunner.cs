using RobotRuntime.Assets;
using RobotRuntime.Commands;
using RobotRuntime.Execution;
using RobotRuntime.Graphics;
using RobotRuntime.Utils;
using RobotRuntime.Utils.Win32;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RobotRuntime
{
    public class TestRunner
    {
        static private TestRunner m_Instance = new TestRunner();
        static public TestRunner Instance { get { return m_Instance; } }
        private TestRunner() { }

        public event Action Finished;
        public event CommandRunningCallback RunningCommand;

        private ValueWrapper<bool> ShouldCancelRun = new ValueWrapper<bool>(false);

        public void Start(LightScript lightScript)
        {
            RunnerFactory.Callback = RunningCommand;

            ShouldCancelRun.Value = false;
            RunnerFactory.CancellingPointerPlaceholder = ShouldCancelRun;

            AssetGuidManager.Instance.LoadMetaFiles();

            //RuntimeSettings.ApplySettings();
            ScreenStateThread.Instace.Start();
            FeatureDetectionThread.Instace.Start();
            Task.Delay(80).Wait(); // make sure first screenshot is taken before starting running commands


            new Thread(delegate ()
            {
                Console.WriteLine("Script start");

                var runner = RunnerFactory.CreateFor(lightScript.GetType());
                runner.Run(lightScript);

                ScreenStateThread.Instace.Stop();
                FeatureDetectionThread.Instace.Stop();

                Finished?.Invoke();
                Console.WriteLine("End script.");
            }).Start();
        }

        public void Stop()
        {
            ShouldCancelRun.Value = true;
        }
    }
}
