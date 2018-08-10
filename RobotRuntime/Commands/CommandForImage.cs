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
        public int Timeout { get; set; }

        public CommandForImage() : base() { }
        public CommandForImage(Guid guid) : base(guid) { }
        public CommandForImage(Guid asset, int timeOut, Guid guid = default(Guid)) : base(guid)
        {
            Asset = asset;
            Timeout = timeOut;
        }

        public override object Clone()
        {
            return new CommandForImage(Asset, Timeout, Guid);
        }

        public override void Run(TestData TestData) { }

        public override string ToString()
        {
            return "For Image: <" + Asset.ToString() + ">";
        }
    }
}
