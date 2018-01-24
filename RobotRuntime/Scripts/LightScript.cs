using RobotRuntime.Execution;
using RobotRuntime.Utils;
using System;
namespace RobotRuntime
{
    [Serializable]
    public class LightScript : IRunnable
    {
        public LightScript() { }

        public LightScript(TreeNode<Command> commands)
        {
            Commands = commands;
        }

        public TreeNode<Command> Commands { get; protected set; }

        public void Run(IRunner runner)
        {
            runner.Run(this);
        }
    }
}
