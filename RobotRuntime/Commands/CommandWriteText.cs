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

        public CommandWriteText() : base() { }
        public CommandWriteText(Guid guid) : base(guid) { }
        public CommandWriteText(string Text, Guid guid = default(Guid)) : base(guid)
        {
            this.Text = Text;
        }

        public override object Clone()
        {
            return new CommandWriteText(Text, Guid);
        }

        public override void Run(TestData TestData)
        {
            WinAPI.SimulateTextEntry(Text);
        }

        public override string Title
        {
            get
            {
                return "Write text: " + Text;
            }
        }
    }
}
