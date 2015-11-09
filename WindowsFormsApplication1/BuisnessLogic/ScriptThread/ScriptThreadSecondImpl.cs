using MouseRobotUI.BuisnessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MouseRobot
{
    [Obsolete("Used for presentational purposes, to show DI, do not use in development.")]
    public class ScriptThreadSecondImpl// : IScriptThread
    {
        /*
        public delegate void EventHandler(object sender, CustomEventArgs e);
        public event EventHandler BreakEvent;

        void OnBreakEvent(object sender, CustomEventArgs e)
        {
            e.message = "Breaking script...\nEnd script.";
        }

        public void Start(IEnumerable<ICommand> list, int repeatTimes)
        {
            new Thread(delegate ()
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
                            return;
                        }
                    }
                }
                Console.WriteLine("End script.");
            }).Start();
        }

        event System.EventHandler IScriptThread.BreakEvent { add { } remove { } }

        void IScriptThread.OnBreakEvent(object sender, CustomEventArgs e)
        {
            throw new NotImplementedException();
        }
        */
    }

}
