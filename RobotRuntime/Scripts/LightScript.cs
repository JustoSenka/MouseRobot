using RobotRuntime.Execution;
using RobotRuntime.Tests;
using System;
namespace RobotRuntime
{
    [Serializable]
    [RunnerType(typeof(ScriptRunner))] 
    public class LightScript : IRunnable
    {
        public LightScript(Guid guid = default(Guid))
        {
            Guid = guid == default(Guid) ? Guid.NewGuid() : guid;
        }

        public LightScript(TreeNode<Command> commands, Guid guid = default(Guid)) : this(guid)
        {
            Commands = commands;
        }

        public Guid Guid { get; protected set; }

        public virtual string Name { get; set; }
        public TreeNode<Command> Commands { get; protected set; }

        public void Run(TestData TestData) { }
    }
}
