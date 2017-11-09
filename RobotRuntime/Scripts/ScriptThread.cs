using RobotRuntime.Graphics;
using RobotRuntime.Settings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RobotRuntime
{
    public class ScriptThread
    {
        static private ScriptThread m_Instance = new ScriptThread();
        static public ScriptThread Instance { get { return m_Instance; } }
        private ScriptThread() { }

        public event Action Finished;

        private bool m_Run;

        public void Start(LightScript lightScript)
        {
            m_Run = true;

            //RuntimeSettings.ApplySettings();
            ScreenStateThread.Instace.Start();
            FeatureDetectionThread.Instace.Start();
            Task.Delay(80).Wait(); // make sure first screenshot is taken before starting running commands

            new Thread(delegate ()
            {
                Console.WriteLine("Script start");
                foreach (var v in lightScript.Commands)
                {
                    Console.WriteLine(v.ToString());
                    v.Run();

                    if (!m_Run)
                        break;
                }

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
