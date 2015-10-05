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
        IList<Command> list = new List<Command>();
        
        public delegate void EventHandler(object sender, EventArgs e);
        public event EventHandler BreakEvent;

        public virtual void OnBreakEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Breaking script...");
            Console.WriteLine("End script.");
        }

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

        public Func<bool> CheckIfPointerOffScreen = () => WinAPI.GetCursorPosition().Y < 5;
    }
}
