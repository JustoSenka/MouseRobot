using RobotRuntime.Execution;
using RobotRuntime.Tests;
using RobotRuntime.Utils.Win32;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(SimpleCommandRunner))]
    [PropertyDesignerType("NativeCommandProperties")]
    public class CommandClick : Command
    {
        public override string Name { get { return "Click"; } }
        public override bool CanBeNested { get { return false; } }

        public int X { get; set; }
        public int Y { get; set; }
        public bool DontMove { get; set; }
        public MouseButton MouseButton { get; set; }

        public CommandClick() : base() { }
        public CommandClick(Guid guid) : base(guid) { }
        public CommandClick(int x, int y, bool DontMove, MouseButton MouseButton, Guid guid = default(Guid)) : base(guid)
        {
            X = x;
            Y = y;
            this.DontMove = DontMove;
            this.MouseButton = MouseButton;
        }

        public override object Clone()
        {
            return new CommandClick(X, Y, DontMove, MouseButton, Guid);
        }

        public override void Run(TestData TestData)
        {
            if (!DontMove)
                WinAPI.MouseMoveTo(X, Y);
            WinAPI.PerformActionDown(MouseButton);
            WinAPI.PerformActionUp(MouseButton);
        }

        public override string Title
        {
            get
            {
                var str = DontMove ? "Click Mouse Here" : "Click on: (" + X + ", " + Y + ")";
                return MouseButton.ToString() + " " + str;
            }
        }
    }
}
