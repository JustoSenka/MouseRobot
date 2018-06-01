using RobotRuntime.Execution;
using RobotRuntime.Utils.Win32;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(SimpleCommandRunner))]
    [PropertyDesignerType("NativeCommandProperties")]
    public class CommandPress : Command
    {
        public override string Name { get { return "Press"; } }
        public override bool CanBeNested { get { return false; } }

        public int X { get; set; }
        public int Y { get; set; }
        public bool DontMove { get; set; }

        public CommandPress() { }
        public CommandPress(int x, int y, bool DontMove)
        {
            X = x;
            Y = y;
            this.DontMove = DontMove;
        }

        public override object Clone()
        {
            return new CommandPress(X, Y, DontMove);
        }

        public override void Run()
        {
            if (!DontMove)
                WinAPI.MouseMoveTo(X, Y);
            WinAPI.PerformAction(WinAPI.MouseEventFlags.LeftDown);
            WinAPI.PerformAction(WinAPI.MouseEventFlags.LeftUp);
        }

        public override string ToString()
        {
            if (DontMove)
                return "Click Mouse Here";
            else
                return "Click on: (" + X + ", " + Y + ")";
        }
    }
}
