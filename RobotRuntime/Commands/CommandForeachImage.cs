using RobotRuntime.Execution;
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

        public CommandForeachImage() { }
        public CommandForeachImage(Guid asset, int timeOut)
        {
            Asset = asset;
            Timeout = timeOut;
        }

        public override object Clone()
        {
            return new CommandForeachImage(Asset, Timeout);
        }

        public override void Run()
        {

        }

        public override string ToString()
        {
            return "For Each image: <" + Asset.ToString() + ">";
        }
    }
}
