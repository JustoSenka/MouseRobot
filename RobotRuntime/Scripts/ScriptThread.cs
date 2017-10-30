using System;
using System.Threading;

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
            new Thread(delegate ()
            {
                Console.WriteLine("Script start");
                foreach (var v in lightScript.Commands)
                {
                    Console.WriteLine(v.Text);
                    v.Run();

                    if (!m_Run)
                        break;
                }

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
