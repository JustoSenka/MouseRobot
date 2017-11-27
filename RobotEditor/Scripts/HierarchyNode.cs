using Robot;
using Robot.Scripts;
using RobotRuntime;
using System;
using System.Collections.Generic;

namespace RobotEditor.Scripts
{
    public class HierarchyNode
    {
        public int Level { get; private set; }

        public object Value { get; private set; }
        public Command Command { get; private set; }
        public Script Script { get; private set; }

        public List<HierarchyNode> Children { get; private set; }
        public HierarchyNode Parent { get; set; }

        public HierarchyNode(Command command, HierarchyNode parent)
        {
            Value = command;
            Command = command;
            Parent = parent;
            Level = Parent.Level + 1;

            Children = new List<HierarchyNode>();
        }

        public HierarchyNode(Script script)
        {
            Value = script;
            Script = script;
            Level = 0;

            Children = new List<HierarchyNode>();

            foreach (var node in script)
                Children.Add(new HierarchyNode(node.value, this));
        }

        public void Update(Command command)
        {
            Command = command;
            Value = command;
        }

        public void Update(Script script)
        {
            Script = script;
            Value = script;
        }
    }
}
