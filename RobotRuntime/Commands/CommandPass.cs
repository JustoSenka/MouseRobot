using RobotRuntime.Execution;
using RobotRuntime.Tests;
using RobotRuntime.Utils.Win32;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(SimpleCommandRunner))]
    [PropertyDesignerType("NativeCommandProperties")]
    public class CommandPass : Command
    {
        public override string Name { get { return "Pass"; } }
        public override bool CanBeNested { get { return false; } }

        public CommandPass() : base() { }
        public CommandPass(Guid guid) : base(guid) { }

        public override object Clone()
        {
            return new CommandPass(Guid);
        }

        public override void Run(TestData TestData)
        {
            TestData.TestStatus = TestStatus.Passed;
        }

        public override string Title => "Pass Test";
    }
}
