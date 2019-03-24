﻿using RobotRuntime.Execution;
using RobotRuntime.Tests;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(ImageCommandRunner))]
    [PropertyDesignerType("NativeCommandProperties")]
    public class CommandForImage : Command
    {
        public override string Name { get { return "For Image"; } }
        public override bool CanBeNested { get { return true; } }

        public Guid Asset { get; set; }
        public int Timeout { get; set; } = 1000;
        public bool ForEach { get; set; }

        public CommandForImage() : base() { }
        public CommandForImage(Guid guid) : base(guid) { }
        public CommandForImage(Guid asset, int timeOut, bool forEach, Guid guid = default(Guid)) : base(guid)
        {
            Asset = asset;
            Timeout = timeOut;
            ForEach = forEach;
        }

        public override object Clone()
        {
            return new CommandForImage(Asset, Timeout, ForEach, Guid);
        }

        public override void Run(TestData TestData) { }

        public override string ToString()
        {
            var f = ForEach ? "Each " : "";
            return $"For {f}Image: <{Asset.ToString()}>";
        }
    }
}
