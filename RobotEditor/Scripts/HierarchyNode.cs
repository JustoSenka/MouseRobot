using Robot;
using Robot.Scripts;
using RobotRuntime;
using RobotRuntime.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RobotEditor.Scripts
{
    public class HierarchyNode : IEnumerable<HierarchyNode>
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
                AddChildRecursively(node);
        }

        private void AddChildRecursively(TreeNode<Command> commandNode)
        {
            var newHierarchyNode = new HierarchyNode(commandNode.value, this);
            Children.Add(newHierarchyNode);

            foreach (var childNode in commandNode)
                newHierarchyNode.AddChildRecursively(childNode);
        }

        public HierarchyNode TopLevelScriptNode
        {
            get
            {
                var node = this;
                while (node.Level != 0)
                    node = node.Parent;

                return node;
            }
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

        public HierarchyNode GetNodeFromValue(Command command)
        {
            return GetAllNodes().FirstOrDefault(n => n.Value.Equals(command));
        }

        /// <summary>
        /// Returns all nodes in the tree hierarchy recursivelly, including all child and grandchild nodes
        /// </summary>
        public IEnumerable<HierarchyNode> GetAllNodes(bool includeSelf = true)
        {
            if (includeSelf)
                yield return this;

            foreach (var c in Children)
            {
                yield return c;
                foreach (var child in c.GetAllNodes(false))
                    yield return child;
            }
        }

        public IEnumerator<HierarchyNode> GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return HierarchyNodeStringConverter.ToString(this);
        }
    }
}
