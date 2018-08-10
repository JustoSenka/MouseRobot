using RobotRuntime.Execution;
using RobotRuntime.Tests;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(ImageCommandRunner))]
    [PropertyDesignerType("NativeCommandProperties")]
    public class CommandForeachImage : Command
    {
        public override string Name { get { return "For Each Image"; } }
        public override bool CanBeNested { get { return true; } }

        public Guid Asset { get; set; }
        public int Timeout { get; set; }

        public CommandForeachImage() : base() { }
        public CommandForeachImage(Guid guid) : base(guid) { }
        public CommandForeachImage(Guid asset, int timeOut, Guid guid = default(Guid)) : base(guid)
        {
            Asset = asset;
            Timeout = timeOut;
        }

        public override object Clone()
        {
            return new CommandForeachImage(Asset, Timeout, Guid);
        }

        public override void Run(TestData TestData) { }

        public override string ToString()
        {
            return "For Each image: <" + Asset.ToString() + ">";
        }
    }
}
