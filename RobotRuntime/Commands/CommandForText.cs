using RobotRuntime.Execution;
using RobotRuntime.Tests;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    // [RunnerType(typeof(TextCommandRunner))]
    [PropertyDesignerType("NativeCommandProperties")]
    public class CommandForText : Command
    {
        public override string Name { get { return "For Text"; } }
        public override bool CanBeNested { get { return true; } }

        public int Timeout { get; set; } = 1000;
        public bool ForEach { get; set; }
        public string Text { get; set; } = "";

        public CommandForText() : base() { }
        public CommandForText(Guid guid) : base(guid) { }
        public CommandForText(int timeOut, bool forEach, string Text = "", Guid guid = default(Guid)) : base(guid)
        {
            Timeout = timeOut;
            ForEach = forEach;
            this.Text = Text;
        }

        public override object Clone()
        {
            return new CommandForText(Timeout, ForEach, Text, Guid);
        }

        public override void Run(TestData TestData) { }

        public override string ToString()
        {
            var f = ForEach ? "Each " : "";
            return $"For " + ForEach + "Text: " + Text;
        }
    }
}
