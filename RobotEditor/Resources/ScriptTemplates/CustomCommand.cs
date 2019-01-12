using RobotRuntime;
using System;
using RobotRuntime.Tests;
using RobotRuntime.Execution;

namespace RobotEditor.Resources.ScriptTemplates
{
    [Serializable]
    // [RunnerType(typeof(CustomCommandRunner))] // Can also use already implemented types: SimpleCommandRunner etc.
    // [PropertyDesignerType("CustomCommandDesigner")] // Optional, will specify how to draw command in inspector
    public class CustomCommand : Command
    {
        // This is what will appear in dropdown in inspector under Command Type. Must be unique
        public override string Name { get { return "Custom Command"; } }
        public override bool CanBeNested { get { return true; } }

        public int SomeInt { get; set; } = 5;

        // having an empty constructor is a must, will not work otherwise
        public CustomCommand() { }
        public CustomCommand(int SomeInt)
        {
            this.SomeInt = SomeInt;
        }

        // Having a cloning mechanism is useful for performance or if there are problems. Default one uses reflection to clone all fields
        public override object Clone()
        {
            return new CustomCommand(SomeInt);
        }

        public override void Run(TestData TestData)
        {
            // TODO: RUN METHOD
            // Something could be done here, if it's more complex, CustomCommandRunner can handle it
        }

        public override string ToString()
        {
            // This is what hierarchy will show
            return "Custom Command " + SomeInt;
        }
    }
}
