using RobotRuntime.Utils.Win32;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    public class CommandDown : Command
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool DontMove { get; set; }

        public CommandDown(int x, int y, bool DontMove)
        {
            X = x;
            Y = y;
            this.DontMove = DontMove;
        }

        public override object Clone()
        {
            return new CommandDown(X, Y, DontMove);
        }

        public override void Run()
        {
            if (!DontMove)
                WinAPI.MouseMoveTo(X, Y);
            WinAPI.PerformAction(WinAPI.MouseEventFlags.LeftDown);
        }

        public override string ToString()
        {
            if (DontMove)
                return "Hold Mouse Down";
            else
                return "Down on: (" + X + ", " + Y + ")";
        }

        public override CommandType CommandType { get { return CommandType.Down; } }
    }
}
