using RobotRuntime.Execution;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(ImageCommandRunner))] 
    public class CommandForImage : Command
    {
        public Guid Asset { get; set; }
        public int Timeout { get; set; }

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

        public override CommandType CommandType { get { return CommandType.ForImage; } }
    }
}
