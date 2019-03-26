using RobotRuntime.Execution;
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
        public string DetectionMode { get; set; } = "Default";

        public CommandForImage() : base() { }
        public CommandForImage(Guid guid) : base(guid) { }
        public CommandForImage(Guid asset, int timeOut, bool forEach, string DetectionMode = "Default", Guid guid = default(Guid)) : base(guid)
        {
            Asset = asset;
            Timeout = timeOut;
            ForEach = forEach;
            this.DetectionMode = DetectionMode;
        }

        public override object Clone()
        {
            return new CommandForImage(Asset, Timeout, ForEach, DetectionMode, Guid);
        }

        public override void Run(TestData TestData) { }

        public override string ToString()
        {
            var f = ForEach ? "Each " : "";
            return $"For {f}Image: <{Asset.ToString()}>";
        }
    }
}
