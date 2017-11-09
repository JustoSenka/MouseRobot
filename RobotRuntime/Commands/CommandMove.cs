using RobotRuntime.Utils.Win32;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    public class CommandMove : Command
    {
        public int X { get; set; }
        public int Y { get; set; }

        public CommandMove(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override object Clone()
        {
            return new CommandMove(X, Y);
        }

        public override void Run()
        {
            int x1, y1;
            x1 = WinAPI.GetCursorPosition().X;
            y1 = WinAPI.GetCursorPosition().Y;

            for (int i = 1; i <= 50; i++)
            {
                WinAPI.MouseMoveTo(x1 + ((X - x1) * i / 50), y1 + ((Y - y1) * i / 50));
            }
        }

        public override string ToString()
        {
            return "Move to: (" + X + ", " + Y + ")";
        }
    }
}
