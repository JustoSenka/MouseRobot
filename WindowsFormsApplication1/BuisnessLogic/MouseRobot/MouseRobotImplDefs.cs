using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MouseRobot
{
    public partial class MouseRobotImpl
    {
        IList<ICommand> list = new List<ICommand>();

        public delegate void Action<in T1, in T2>(T1 t1, T2 t2);
        public delegate void Action<in T>(T t);

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
                scriptThread.BreakEvent += scriptThread.OnBreakEvent;
            }
            return WinAPI.GetCursorPosition().Y < 5;
        }
    }
}
