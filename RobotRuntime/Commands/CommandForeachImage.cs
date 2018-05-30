using RobotRuntime.Assets;
using RobotRuntime.Execution;
using RobotRuntime.Graphics;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(ImageCommandRunner))]
    public class CommandForeachImage : Command
    {
        public Guid Asset { get; set; }
        public int Timeout { get; set; }

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

        public override CommandType CommandType { get { return CommandType.ForeachImage; } }
    }
}
