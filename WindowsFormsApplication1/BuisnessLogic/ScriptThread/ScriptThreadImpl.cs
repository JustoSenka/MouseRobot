using MouseRobotUI.BuisnessLogic;
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
        public delegate void MyEventHandler(object sender, CustomEventArgs e);
        public event MyEventHandler BreakEvent;

        void OnBreakEvent(object sender, CustomEventArgs e)
        {
            e.message = "Breaking script...\nEnd script.";
        }

        public void Start(IEnumerable<ICommand> list, int repeatTimes)
        {
            new Thread(delegate()
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
                            BreakEvent -= new MyEventHandler(OnBreakEvent);
                            return;
                        }
                    }
                }
                Console.WriteLine("End script.");
            }).Start();
        }

        event MouseRobot.MyEventHandler IScriptThread.BreakEvent
        {
            add { }
            remove { }
        }

        void IScriptThread.OnBreakEvent(object sender, CustomEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
