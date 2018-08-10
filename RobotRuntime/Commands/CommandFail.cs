using RobotRuntime.Execution;
using RobotRuntime.Tests;
using RobotRuntime.Utils.Win32;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(SimpleCommandRunner))]
    [PropertyDesignerType("NativeCommandProperties")]
    public class CommandFail : Command
    {
        public override string Name { get { return "Fail"; } }
        public override bool CanBeNested { get { return false; } }

        public CommandFail() : base() { }
        public CommandFail(Guid guid) : base(guid) { }

        public override object Clone()
        {
            return new CommandFail(Guid);
        }

        public override void Run(TestData TestData)
        {
            TestData.ShouldFailTest = true;
        }

        public override string ToString()
        {
            return "Fail Test";
        }
    }
}
