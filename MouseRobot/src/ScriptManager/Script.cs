using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Robot
{
    [Serializable]
    public class Script : ICloneable
    {
        public IList<Command> commands;
        public string path;

        public IEnumerable<string> CommandText
        {
            get
            {
                return commands.Select((c) => c.Text);
            }
        }

        public string Name
        {
            get
            {
                if (path == "")
                {
                    return "New Script";
                }
                else
                {
                    var split = path.Split('/', '\\', '.');
                    return split[split.Length - 2];
                }
            }
        }

        public Script()
        {
            commands = new List<Command>();
            path = "";
        }

        public void MoveCommandAfter(int command, int commandAfter)
        {

        }

        public void AddCommandSleep(int time)
        {
            commands.Add(new Command(() =>
            {
                Thread.Sleep(time);
                CheckIfPointerOffScreen();
            }, "Sleep for " + time + " ms.", CommandCode.G, time));
        }

        public void AddCommandRelease()
        {
            commands.Add(new Command(() =>
            {
                MouseAction(WinAPI.MouseEventFlags.LeftUp);
                CheckIfPointerOffScreen();
            }, "Release", CommandCode.K));
        }

        public void AddCommandPress(int x, int y)
        {
            commands.Add(new Command(delegate ()
            {
                MouseMoveTo(x, y);
                MouseAction(WinAPI.MouseEventFlags.LeftDown);
                MouseAction(WinAPI.MouseEventFlags.LeftUp);
                CheckIfPointerOffScreen();
            }, "Press on: (" + x + ", " + y + ")", CommandCode.S, x, y));
        }

        public void AddCommandMove(int x, int y)
        {
            commands.Add(new Command(delegate ()
            {
                int x1, y1;
                x1 = WinAPI.GetCursorPosition().X;
                y1 = WinAPI.GetCursorPosition().Y;

                for (int i = 1; i <= 50; i++)
                {
                    MouseMoveTo(x1 + ((x - x1) * i / 50), y1 + ((y - y1) * i / 50));
                }
                CheckIfPointerOffScreen();
            }, "Move to: (" + x + ", " + y + ")", CommandCode.J, x, y));
        }

        public void AddCommandDown(int x, int y)
        {
            commands.Add(new Command(delegate ()
            {
                MouseMoveTo(x, y);
                MouseAction(WinAPI.MouseEventFlags.LeftDown);
                CheckIfPointerOffScreen();
            }, "Down on: (" + x + ", " + y + ")", CommandCode.H, x, y));
        }

        public void EmptyScript()
        {
            commands.Clear();
            Console.WriteLine("List is empty.");
        }

        public Action<int, int> MouseMoveTo = (int x, int y) =>
        {
            WinAPI.SetCursorPosition(x, y);
            Thread.Sleep(WinAPI.TimeBetweenActions);
        };

        public Action<WinAPI.MouseEventFlags> MouseAction = (WinAPI.MouseEventFlags flags) =>
        {
            WinAPI.MouseEvent(flags);
            Thread.Sleep(WinAPI.TimeBetweenActions);
        };

        public bool CheckIfPointerOffScreen()
        {
            if (WinAPI.GetCursorPosition().Y < 5)
            {
                // Stop the script here
            }
            return WinAPI.GetCursorPosition().Y < 5;
        }

        public object Clone()
        {
            var script = new Script();

            foreach (var v in commands)
            {
                switch (v.Code)
                {
                    case CommandCode.G:
                        script.AddCommandSleep(v.Args.ElementAt(0));
                        break;
                    case CommandCode.S:
                        script.AddCommandPress(v.Args.ElementAt(0), v.Args.ElementAt(1));
                        break;
                    case CommandCode.H:
                        script.AddCommandDown(v.Args.ElementAt(0), v.Args.ElementAt(1));
                        break;
                    case CommandCode.J:
                        script.AddCommandMove(v.Args.ElementAt(0), v.Args.ElementAt(1));
                        break;
                    case CommandCode.K:
                        script.AddCommandRelease();
                        break;
                }
            }

            return script;
        }
    }
}
