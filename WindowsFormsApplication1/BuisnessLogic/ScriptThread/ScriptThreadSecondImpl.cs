using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MouseRobot
{
    [Obsolete("Used for presentational purposes, to show DI, do not use in development.")]
    public class ScriptThreadSecondImpl : IScriptThread
    {
        //public delegate void EventHandler(object sender, EventArgs e);
        public event EventHandler BreakEvent;

        public void OnBreakEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Breaking script...");
            Console.WriteLine("It should break, but my poor programmer made lots of mistakes");
        }

        private Thread t;
        private IEnumerable<ICommand> list;
        private int repeatTimes;

        public ScriptThreadSecondImpl()
        {
            t = new Thread(delegate()
            {
                for (int i = 1; i <= repeatTimes; i++)
                {
                    Console.WriteLine(i + " - Script start");
                    foreach (var v in list)
                    {
                        Console.WriteLine(v.Text);
                        v.Run();

                        if (BreakEvent != null)
                        {
                            BreakEvent.Invoke(this, null);
                            BreakEvent -= new EventHandler(OnBreakEvent);
                            //return;
                        }
                    }
                }
                Console.WriteLine("End script.");
            });
        }

        public void Start(IEnumerable<ICommand> list, int repeatTimes)
        {
            this.list = list;
            this.repeatTimes = repeatTimes;
            t.Start();
        }
    }
}
