using RobotRuntime.Execution;
using RobotRuntime.Tests;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(ForTextCommandRunner))]
    [PropertyDesignerType("NativeCommandProperties")]
    public class CommandForText : Command
    {
        public override string Name { get { return "For Text"; } }
        public override bool CanBeNested { get { return true; } }

        public string Text { get; set; } = "";
        public string TextDetectionMode { get; set; } = "Default";
        public float TextComparisonThreshold { get; set; } = 0.80f;
        public int Timeout { get; set; } = 5000;
        public bool ForEach { get; set; }

        public CommandForText() : base() { }
        public CommandForText(Guid guid) : base(guid) { }
        public CommandForText(int timeOut, bool forEach, string Text = "", float TextComparisonThreshold = 0.80f, string TextDetectionMode = "Default", Guid guid = default(Guid)) : base(guid)
        {
            Timeout = timeOut;
            ForEach = forEach;
            this.Text = Text;
            this.TextComparisonThreshold = TextComparisonThreshold;
            this.TextDetectionMode = TextDetectionMode;
        }

        public override object Clone()
        {
            return new CommandForText(Timeout, ForEach, Text, TextComparisonThreshold, TextDetectionMode, Guid);
        }

        public override void Run(TestData TestData) { }

        public override string ToString()
        {
            var f = ForEach ? "Each " : "";
            return $"For " + f + "Text Block: \"" + Text + "\"";
        }
    }
}
