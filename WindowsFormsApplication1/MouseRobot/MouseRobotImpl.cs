using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MouseRobot
{
    public class MouseRobotImpl : IMouseRobot
    {
        IList<Command> list = new List<Command>();
        public event Action scriptEvent;

        // public delegate void Action<in T1, in T2>(T1 t1, T2 t2);
        // public delegate void Action(int t1, int t2);

        // Func<string, int> sth = delegate(string str) { return Int32.Parse(str); };
        // Func<string, int> sth = (string str) => Int32.Parse(str);

        Action<int, int> MouseMoveTo = (int x, int y) =>
        {
            WinAPI.SetCursorPosition(x, y);
            Thread.Sleep(WinAPI.TimeBetweenActions);
        };

        Action<WinAPI.MouseEventFlags> MouseAction = (WinAPI.MouseEventFlags flags) =>
        {
            WinAPI.MouseEvent(flags);
            Thread.Sleep(WinAPI.TimeBetweenActions);
        };

        public void StartScript(int repeatTimes)
        {
            if (list.Count <= 0)
            {
                throw new EmptyScriptException("List is empty");
            }

            ScriptThread.Start(delegate() 
            {
                for (int i = 1; i <= repeatTimes; i++)
                {
                    Console.WriteLine(i + " - Script start.");
                    foreach (var v in list)
                    {
                        Console.WriteLine(v.Text);
                        v.Run();
                        if (WinAPI.GetCursorPosition().Y < 5)
                        {
                            Console.WriteLine("End script.");
                            return;
                        }
                    }
                }
                Console.WriteLine("End script.");
            });
        }

        public void AddCommandSleep(int time)
        {
            list.Add(new Command(() =>
            {
                Thread.Sleep(time);
            }, "Sleep for " + time + " ms."));

            //scriptEvent += new Action(() => Console.);
        }

        public void AddCommandRelease()
        {
            list.Add(new Command( () => 
            {
                MouseAction(WinAPI.MouseEventFlags.LeftUp);
            }, "Release"));
        }

        public void AddCommandPress(int x, int y)
        {
            list.Add(new Command(delegate()
            {
                MouseMoveTo(x, y);
                MouseAction(WinAPI.MouseEventFlags.LeftDown);
                MouseAction(WinAPI.MouseEventFlags.LeftUp);
            }, "Press on: (" + x + ", " + y + ")"));
        }

        public void AddCommandMove(int x, int y)
        {
            list.Add(new Command(delegate()
            {
                int x1, y1;
                x1 = WinAPI.GetCursorPosition().X;
                y1 = WinAPI.GetCursorPosition().Y;

                for (int i = 1; i <= 50; i++)
                {
                    MouseMoveTo(x1 + ((x - x1) * i / 50), y1 + ((y - y1) * i / 50));
                }
            }, "Move to: (" + x + ", " + y + ")"));
        }

        public void AddCommandDown(int x, int y)
        {
            list.Add(new Command(delegate()
            {
                MouseMoveTo(x, y);
                MouseAction(WinAPI.MouseEventFlags.LeftDown);
            }, "Down on: (" + x + ", " + y + ")"));
        }


        public void EmptyScript()
        {
            list.Clear();
        }
    }
}
