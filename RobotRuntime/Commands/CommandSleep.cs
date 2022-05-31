using RobotRuntime.Execution;
using RobotRuntime.Tests;
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

        public CommandSleep() : base() { }
        public CommandSleep(Guid guid) : base(guid) { }
        public CommandSleep(int millis, Guid guid = default(Guid)) : base(guid)
        {
            Time = millis;
        }

        public override object Clone()
        {
            return new CommandSleep(Time, Guid);
        }

        public override void Run(TestData TestData)
        {
            Task.Delay(Time).Wait();
        }

        public override string Title => "Sleep for " + Time + " ms.";
    }
}
