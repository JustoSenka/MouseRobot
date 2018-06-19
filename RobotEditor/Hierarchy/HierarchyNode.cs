using RobotRuntime;
using RobotRuntime.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RobotEditor.Hierarchy
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

        public HierarchyNode(object value)
        {
            Value = value;
            Children = new List<HierarchyNode>();
        }

        public HierarchyNode(Script script, int overrideLevel = 0)
        {
            Value = script;
            Script = script;
            Level = overrideLevel;

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

        public void AddHierarchyNode(HierarchyNode node)
        {
            node.Parent = this;
            node.Level = Level + 1;
            Children.Add(node);
        }

        public HierarchyNode TopLevelNode
        {
            get
            {
                var node = this;
                while (node.Level != 0)
                    node = node.Parent;

                return node;
            }
        }

        public HierarchyNode TopLevelScriptNode
        {
            get
            {
                HierarchyNode scriptNode = this.Script != null ? this : null;
                var node = this;
                while (node.Level != 0)
                {
                    node = node.Parent;

                    if (node.Script != null)
                        scriptNode = node;
                }

                return scriptNode;
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

            Children = new List<HierarchyNode>();

            foreach (var node in script)
                AddChildRecursively(node);
        }

        public HierarchyNode GetNodeFromValue(Command command)
        {
            return GetAllNodes().FirstOrDefault(n => n.Value.Equals(command));
        }

        /// <summary>
        /// Returns all nodes in the tree hierarchy recursively, including all child and grandchild nodes
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
            return Value.ToString();
        }
    }
}
