using RobotRuntime.Execution;
using RobotRuntime.Tests;
using System;
namespace RobotRuntime
{
    [Serializable]
    [RunnerType(typeof(ScriptRunner))] 
    public class LightScript : IRunnable
    {
        public LightScript() { }

        public LightScript(TreeNode<Command> commands)
        {
            Commands = commands;
        }

        public virtual string Name { get; set; }
        public TreeNode<Command> Commands { get; protected set; }

        public void Run(TestData TestData) { }
    }
}
