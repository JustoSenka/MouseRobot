using RobotRuntime.Execution;
using RobotRuntime.Tests;
using RobotRuntime.Utils.Win32;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(SimpleCommandRunner))]
    [PropertyDesignerType("NativeCommandProperties")]
    public class CommandWriteText : Command
    {
        public override string Name { get { return "Write Text"; } }
        public override bool CanBeNested { get { return false; } }

        public string Text { get; set; }

        public CommandWriteText() { }
        public CommandWriteText(string Text)
        {
            this.Text = Text;
        }

        public override object Clone()
        {
            return new CommandWriteText(Text);
        }

        public override void Run(TestData TestData)
        {
            WinAPI.SimulateTextEntry(Text);
        }

        public override string ToString()
        {
            return "Write text: " + Text;
        }
    }
}
