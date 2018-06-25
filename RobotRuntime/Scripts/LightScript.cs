using RobotRuntime.Execution;
using RobotRuntime.Utils;
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

        public string Name { get; set; }
        public TreeNode<Command> Commands { get; protected set; }

        public void Run(IRunner runner)
        {
            runner.Run(this);
        }
    }
}
