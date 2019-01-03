using RobotRuntime.Execution;
using RobotRuntime.Tests;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(RunRecordingCommandRunner))] 
    [PropertyDesignerType("NativeCommandProperties")]
    public class CommandRunRecording : Command
    {
        public override string Name { get { return "Run Script"; } }
        public override bool CanBeNested { get { return false; } }

        public Guid Asset { get; set; }

        public CommandRunRecording() : base() { }
        public CommandRunRecording(Guid guid) : base(guid) { }
        public CommandRunRecording(Guid script, Guid guid = default(Guid)) : base(guid)
        {
            Asset = script;
        }

        public override object Clone()
        {
            return new CommandRunRecording(Asset, Guid);
        }

        public override void Run(TestData TestData) { }

        public override string ToString()
        {
            return "Run Script: <" + Asset.ToString() + ">";
        }
    }
}
