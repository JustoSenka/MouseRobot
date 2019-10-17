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

        public CommandRelease() : base() { }
        public CommandRelease(Guid guid) : base(guid) { }
        public CommandRelease(int x, int y, bool DontMove, Guid guid = default(Guid)) : base(guid)
        {
            X = x;
            Y = y;
            this.DontMove = DontMove;
        }

        public override object Clone()
        {
            return new CommandRelease(X, Y, DontMove, Guid);
        }

        public override void Run(TestData TestData)
        {
            if (!DontMove)
                WinAPI.MouseMoveTo(X, Y);

            if (WinAPI.CurrentMouseState.HasFlag(WinAPI.MouseEventFlags.LeftDown))
                WinAPI.PerformActionUp(MouseButton.Left);

            if (WinAPI.CurrentMouseState.HasFlag(WinAPI.MouseEventFlags.RightDown))
                WinAPI.PerformActionUp(MouseButton.Right);

            if (WinAPI.CurrentMouseState.HasFlag(WinAPI.MouseEventFlags.MiddleDown))
                WinAPI.PerformActionUp(MouseButton.Middle);

        }

        public override string Title => DontMove ? "Release" : "Release on: (" + X + ", " + Y + ")";
    }
}
