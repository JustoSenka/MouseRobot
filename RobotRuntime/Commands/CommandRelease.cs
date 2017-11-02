using RobotRuntime.Utils.Win32;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    public class CommandRelease : Command
    {
        public int X { get; set; }
        public int Y { get; set; }

        public CommandRelease(int x, int y)
        {
            X = x;
            Y = y;
            Text = "Release on: (" + x + ", " + y + ")";
        } 

        public override object Clone()
        {
            return new CommandRelease(X, Y);
        }

        public override void Run()
        {
            WinAPI.MouseMoveTo(X, Y);
            WinAPI.PerformAction(WinAPI.MouseEventFlags.LeftUp);
        }
    }
}
