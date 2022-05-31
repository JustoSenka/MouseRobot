﻿using RobotRuntime.Execution;
using RobotRuntime.Tests;
using RobotRuntime.Utils.Win32;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(SimpleCommandRunner))]
    [PropertyDesignerType("NativeCommandProperties")]
    public class CommandMove : Command
    {
        public override string Name { get { return "Move"; } }
        public override bool CanBeNested { get { return false; } }

        public int X { get; set; }
        public int Y { get; set; }

        public CommandMove() : base() { }
        public CommandMove(Guid guid) : base(guid) { }
        public CommandMove(int x, int y, Guid guid = default(Guid)) : base(guid)
        {
            X = x;
            Y = y;
        }

        public override object Clone()
        {
            return new CommandMove(X, Y, Guid);
        }

        public override void Run(TestData TestData)
        {
            int x1, y1;
            x1 = WinAPI.GetCursorPosition().X;
            y1 = WinAPI.GetCursorPosition().Y;

            for (int i = 1; i <= 50; i++)
            {
                WinAPI.MouseMoveTo(x1 + ((X - x1) * i / 50), y1 + ((Y - y1) * i / 50));
            }
        }

        public override string Title => "Move to: (" + X + ", " + Y + ")";
    }
}
