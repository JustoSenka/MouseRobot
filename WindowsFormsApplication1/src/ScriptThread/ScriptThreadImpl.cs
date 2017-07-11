using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MouseRobot
{
    public class ScriptThreadImpl : IScriptThread
    {
        public void Start(Script script, int repeatTimes)
        {
            new Thread(delegate()
            {
                for (int i = 1; i <= repeatTimes; i++)
                {
                    Console.WriteLine(i + " - Script start");
                    foreach (var v in script.commands)
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
