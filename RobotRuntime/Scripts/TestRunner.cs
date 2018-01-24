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

        private bool m_Run;

        public void Start(LightScript lightScript)
        {
            m_Run = true;
            RunnerFactory.Callback = RunningCommand;

            AssetGuidManager.Instance.LoadMetaFiles();

            //RuntimeSettings.ApplySettings();
            ScreenStateThread.Instace.Start();
            FeatureDetectionThread.Instace.Start();
            Task.Delay(80).Wait(); // make sure first screenshot is taken before starting running commands


            new Thread(delegate ()
            {
                Console.WriteLine("Script start");

                var runner = new ScriptRunner(RunningCommand);
                runner.Run(lightScript);

                ScreenStateThread.Instace.Stop();
                FeatureDetectionThread.Instace.Stop();

                Finished?.Invoke();
                Console.WriteLine("End script.");
            }).Start();
        }

        public void Stop()
        {
            m_Run = false;
        }
    }
}
