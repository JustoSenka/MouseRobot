using RobotRuntime.Execution;
using RobotRuntime.Tests;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(RunScriptCommandRunner))] 
    [PropertyDesignerType("NativeCommandProperties")]
    public class CommandRunScript : Command
    {
        public override string Name { get { return "Run Script"; } }
        public override bool CanBeNested { get { return false; } }

        public Guid Asset { get; set; }

        public CommandRunScript() { }
        public CommandRunScript(Guid script)
        {
            Asset = script;
        }

        public override object Clone()
        {
            return new CommandRunScript(Asset);
        }

        public override void Run(TestData TestData) { }

        public override string ToString()
        {
            return "Run Script: <" + Asset.ToString() + ">";
        }
    }
}
