using RobotRuntime.Execution;
using RobotRuntime.Tests;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(SimpleCommandRunner))]
    [PropertyDesignerType("UnknownCommandProperties")]
    public class CommandUnknown : Command
    {
        public override string Name { get { return "Unknown"; } }
        public override bool CanBeNested { get { return true; } }

        public string SerializedText { get; set; }

        public CommandUnknown() : base() { }
        public CommandUnknown(Guid guid) : base(guid) { }
        public CommandUnknown(string SerializedText, Guid guid = default) : base(guid)
        {
            this.SerializedText = SerializedText;
        }

        public override void Run(TestData TestData)
        {
            Logger.Log(LogType.Warning, "This command was incorrectly deserialized and cannot run.");
        }
        
        public override string ToString() => "Unknown command";
    }
}
