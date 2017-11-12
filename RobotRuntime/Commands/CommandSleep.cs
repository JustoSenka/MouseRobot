using System;
using System.Threading.Tasks;

namespace RobotRuntime.Commands
{
    [Serializable]
    public class CommandSleep : Command
    {
        public int Time { get; set; }

        public CommandSleep(int millis)
        {
            Time = millis;
        }

        public override object Clone()
        {
            return new CommandSleep(Time);
        }

        public override void Run()
        {
            Task.Delay(Time).Wait();
        }

        public override string ToString()
        {
            return "Sleep for " + Time + " ms.";
        }

        public override CommandType CommandType { get { return CommandType.Sleep; } }
    }
}
