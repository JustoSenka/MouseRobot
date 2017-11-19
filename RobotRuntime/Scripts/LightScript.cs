using RobotRuntime.Utils;
using System;
namespace RobotRuntime
{
    [Serializable]
    public class LightScript
    {
        public LightScript() { }

        public LightScript(TreeNode<Command> commands)
        {
            Commands = commands;
        }

        public TreeNode<Command> Commands { get; protected set; }
    }
}
