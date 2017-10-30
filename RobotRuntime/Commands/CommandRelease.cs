using RobotRuntime.Utils.Win32;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    class CommandRelease : Command
    {
        public int X { get; set; }
        public int Y { get; set; }

        public CommandRelease()
        {
            Text = "Release";
        }

        public override object Clone()
        {
            return new CommandRelease();
        }

        public override void Run()
        {
            WinAPI.PerformAction(WinAPI.MouseEventFlags.LeftUp);
        }
    }
}
