using RobotRuntime.Utils.Win32;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    public class CommandPress : Command
    {
        public int X { get; set; }
        public int Y { get; set; }

        public CommandPress(int x, int y)
        {
            X = x;
            Y = y;
            Text = "Press on: (" + x + ", " + y + ")";
        }

        public override object Clone()
        {
            return new CommandPress(X, Y);
        }

        public override void Run()
        {
            WinAPI.MouseMoveTo(X, Y);
            WinAPI.PerformAction(WinAPI.MouseEventFlags.LeftDown);
            WinAPI.PerformAction(WinAPI.MouseEventFlags.LeftUp);
        }
    }
}
