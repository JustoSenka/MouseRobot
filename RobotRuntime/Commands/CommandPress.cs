using RobotRuntime.Execution;
using RobotRuntime.Tests;
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
        public MouseButton MouseButton { get; set; }

        public CommandPress() : base() { }
        public CommandPress(Guid guid) : base(guid) { }
        public CommandPress(int x, int y, bool DontMove, MouseButton MouseButton, Guid guid = default(Guid)) : base(guid)
        {
            X = x;
            Y = y;
            this.DontMove = DontMove;
            this.MouseButton = MouseButton;
        }

        public override object Clone()
        {
            return new CommandPress(X, Y, DontMove, MouseButton, Guid);
        }

        public override void Run(TestData TestData)
        {
            if (!DontMove)
                WinAPI.MouseMoveTo(X, Y);
            WinAPI.PerformActionDown(MouseButton);
            WinAPI.PerformActionUp(MouseButton);
        }

        public override string ToString()
        {
            var str = DontMove ? "Click Mouse Here" : "Click on: (" + X + ", " + Y + ")";
            str = MouseButton.ToString() + " " + str;
            return str;
        }
    }
}
