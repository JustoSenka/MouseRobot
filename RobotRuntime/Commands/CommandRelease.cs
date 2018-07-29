using RobotRuntime.Execution;
using RobotRuntime.Tests;
using RobotRuntime.Utils.Win32;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(SimpleCommandRunner))]
    [PropertyDesignerType("NativeCommandProperties")]
    public class CommandRelease : Command
    {
        public override string Name { get { return "Relese"; } }
        public override bool CanBeNested { get { return false; } }

        public int X { get; set; }
        public int Y { get; set; }
        public bool DontMove { get; set; }

        public CommandRelease() { }
        public CommandRelease(int x, int y, bool DontMove)
        {
            X = x;
            Y = y;
            this.DontMove = DontMove;
        }

        public override object Clone()
        {
            return new CommandRelease(X, Y, DontMove);
        }

        public override void Run(TestData TestData)
        {
            if (!DontMove)
                WinAPI.MouseMoveTo(X, Y);
            WinAPI.PerformActionUp(MouseButton.Left);
            WinAPI.PerformActionUp(MouseButton.Right);
            WinAPI.PerformActionUp(MouseButton.Middle);
        }

        public override string ToString()
        {
            if (DontMove)
                return "Release";
            else
                return "Release on: (" + X + ", " + Y + ")";
        }
    }
}
