using RobotRuntime.Execution;
using System;
using System.Threading.Tasks;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(SimpleCommandRunner))]
    [PropertyDesignerType("NativeCommandProperties")]
    public class CommandSleep : Command
    {
        public override string Name { get { return "Sleep"; } }
        public override bool CanBeNested { get { return false; } }

        public int Time { get; set; }

        public CommandSleep() { }
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
    }
}
