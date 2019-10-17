using RobotRuntime.Execution;
using RobotRuntime.Tests;
using RobotRuntime.Utils.Win32;
using System;
using System.Windows.Forms;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(SimpleCommandRunner))]
    [PropertyDesignerType("NativeCommandProperties")]
    public class CommandPressKey : Command
    {
        public override string Name { get { return "Press Key"; } }
        public override bool CanBeNested { get { return false; } }

        public Keys KeyCode { get; set; }

        public CommandPressKey() : base() { }
        public CommandPressKey(Guid guid) : base(guid) { }
        public CommandPressKey(Keys KeyCode, Guid guid = default(Guid)) : base(guid)
        {
            this.KeyCode = KeyCode;
        }

        public override object Clone()
        {
            return new CommandPressKey(KeyCode, Guid);
        }

        public override void Run(TestData TestData)
        {
            WinAPI.PressKey(KeyCode);
        }

        public override string Title => "Press Key: " + KeyCode;
    }
}
