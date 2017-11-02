using RobotRuntime.Utils.Win32;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    public class CommandDown : Command
    {
        public int X { get; set; }
        public int Y { get; set; }

        public CommandDown(int x, int y)
        {
            X = x;
            Y = y;
            Text = "Down on: (" + x + ", " + y + ")";
        }

        public override object Clone()
        {
            return new CommandDown(X, Y);
        }

        public override void Run()
        {
            WinAPI.MouseMoveTo(X, Y);
            WinAPI.PerformAction(WinAPI.MouseEventFlags.LeftDown);
        }
    }
}
