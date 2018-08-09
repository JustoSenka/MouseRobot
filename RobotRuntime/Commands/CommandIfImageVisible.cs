using RobotRuntime.Execution;
using RobotRuntime.Tests;
using System;

namespace RobotRuntime.Commands
{
    [Serializable]
    [RunnerType(typeof(CommandIfImageVisibleRunner))] 
    [PropertyDesignerType("NativeCommandProperties")]
    public class CommandIfImageVisible : Command
    {
        public override string Name { get { return "If Image Visible"; } }
        public override bool CanBeNested { get { return true; } }

        public Guid Asset { get; set; }
        public int Timeout { get; set; }
        public bool ExpectTrue { get; set; }

        public CommandIfImageVisible() { }
        public CommandIfImageVisible(Guid asset, int timeOut, bool ExpectTrue)
        {
            Asset = asset;
            Timeout = timeOut;
            this.ExpectTrue = ExpectTrue;
        }

        public override object Clone()
        {
            return new CommandIfImageVisible(Asset, Timeout, ExpectTrue);
        }

        public override void Run(TestData TestData) { }

        public override string ToString()
        {
            var str = ExpectTrue ? " " : " not ";
            return "If Image is"  + str + "Visible: <" + Asset.ToString() + ">";
        }
    }
}
