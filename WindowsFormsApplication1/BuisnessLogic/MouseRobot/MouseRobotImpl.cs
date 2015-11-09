using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MouseRobotUI.BuisnessLogic;

namespace MouseRobot
{
    public partial class MouseRobotImpl : IMouseRobot
    {
        public delegate void MyEventHandler(object sender, CustomEventArgs e);

        public IScriptThread scriptThread;
        public MouseRobotImpl(IScriptThread scriptThread) 
        {
            this.scriptThread = scriptThread;
        }

        public void StartScript(int repeatTimes)
        {
            if (list.Count <= 0)
            {
                throw new EmptyScriptException("Script is empty");
            }
            scriptThread.Start(list, repeatTimes);
        }

        public void StopScript()
        {
            if (list.Count != 0)
            {
                scriptThread.BreakEvent += scriptThread.OnBreakEvent;
            }
        }

        public void AddCommandSleep(int time)
        {
            list.Add(DependencyInjector.GetCommand(() =>
            {
                Thread.Sleep(time);
                CheckIfPointerOffScreen();
            }, "Sleep for " + time + " ms.", CommandCode.G, time));
        }

        public void AddCommandRelease()
        {
            list.Add(DependencyInjector.GetCommand(() => 
            {
                MouseAction(WinAPI.MouseEventFlags.LeftUp);
                CheckIfPointerOffScreen();
            }, "Release", CommandCode.K));
        }

        public void AddCommandPress(int x, int y)
        {
            list.Add(DependencyInjector.GetCommand(delegate()
            {
                MouseMoveTo(x, y);
                MouseAction(WinAPI.MouseEventFlags.LeftDown);
                MouseAction(WinAPI.MouseEventFlags.LeftUp);
                CheckIfPointerOffScreen();
            }, "Press on: (" + x + ", " + y + ")", CommandCode.S, x, y));
        }

        public void AddCommandMove(int x, int y)
        {
            list.Add(DependencyInjector.GetCommand(delegate()
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
            list.Add(DependencyInjector.GetCommand(delegate()
            {
                MouseMoveTo(x, y);
                MouseAction(WinAPI.MouseEventFlags.LeftDown);
                CheckIfPointerOffScreen();
            }, "Down on: (" + x + ", " + y + ")", CommandCode.H, x, y));
        }

        public void EmptyScript()
        {
            list.Clear();
            Console.WriteLine("List is empty.");
        }

        public void Open(string fileName)
        {
            IList<ICommand> tempList = BinaryObjectIO.LoadObject<IList<ICommand>>(fileName);
            list.Clear();

            Console.WriteLine();
            Console.WriteLine("Reading file:");

            foreach (var v in tempList)
            {
                switch (v.Code) 
                {
                    case CommandCode.G:
                        AddCommandSleep(v.Args.ElementAt<int>(0));
                        break;
                    case CommandCode.S:
                        AddCommandPress(v.Args.ElementAt<int>(0), v.Args.ElementAt<int>(1));
                        break;
                    case CommandCode.H:
                        AddCommandDown(v.Args.ElementAt<int>(0), v.Args.ElementAt<int>(1));
                        break;
                    case CommandCode.J:
                        AddCommandMove(v.Args.ElementAt<int>(0), v.Args.ElementAt<int>(1));
                        break;
                    case CommandCode.K:
                        AddCommandRelease();
                        break;
                }

                Console.WriteLine(v.Text);
            }
        }

        public void Save(string fileName)
        {
            foreach (var v in list)
            {
                v.ClearMethod();
            }
            BinaryObjectIO.SaveObject<IList<ICommand>>(fileName, list);

            Open(fileName);
            Console.WriteLine("File saved.");
        }
    }
}
