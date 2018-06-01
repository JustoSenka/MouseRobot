using RobotRuntime.Execution;
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

        public CommandForImage() { }
        public CommandForImage(Guid asset, int timeOut)
        {
            Asset = asset;
            Timeout = timeOut;
        }

        public override object Clone()
        {
            return new CommandForImage(Asset, Timeout);
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
