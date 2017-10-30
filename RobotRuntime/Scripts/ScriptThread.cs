using System;
using System.Threading;

namespace RobotRuntime
{
    public class ScriptThread
    {
        static private ScriptThread m_Instance = new ScriptThread();
        static public ScriptThread Instance { get { return m_Instance; } }
        private ScriptThread() { }

        public void Start(Script script, int repeatTimes)
        {
            new Thread(delegate()
            {
                for (int i = 1; i <= repeatTimes; i++)
                {
                    Console.WriteLine(i + " - Script start");
                    foreach (var v in script.Commands)
                    {
                        Console.WriteLine(v.Text);
                        v.Run();
                        // BREAK THE THREAD HERE SOMEWHERE
                    }
                }
                Console.WriteLine("End script.");
            }).Start();
        }
    }
}
