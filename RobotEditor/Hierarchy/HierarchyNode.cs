using Robot.Recordings;
using RobotRuntime;
using RobotRuntime.Recordings;
using System;
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
        public Recording Recording { get; private set; }

        public List<HierarchyNode> Children { get; private set; }
        public HierarchyNode Parent { get; set; }

        // Properties used for drag and drop

        private HierarchyNodeDropDetails m_DropDetails;
        public HierarchyNodeDropDetails DropDetails
        {
            get { return m_DropDetails; }
            set
            {
                if (value == default)
                    return;

                m_DropDetails = value;
                if (Children == null)
                    return;

                foreach (var c in Children)
                    c.DropDetails = value;
            }
        }

        public HierarchyNode(Command command, HierarchyNode parent)
        {
            Value = command;
            Command = command;
            Parent = parent;
            Level = Parent.Level + 1;
            DropDetails = parent.DropDetails;

            Children = new List<HierarchyNode>();
        }

        public HierarchyNode(object value, HierarchyNodeDropDetails dropDetails)
        {
            Value = value;
            Children = new List<HierarchyNode>();
        }

        public HierarchyNode(Recording recording, HierarchyNodeDropDetails dropDetails, int overrideLevel = 0)
        {
            Value = recording;
            Recording = recording;
            Level = overrideLevel;
            DropDetails = dropDetails;

            Children = new List<HierarchyNode>();

            foreach (var node in recording)
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
            node.DropDetails = DropDetails;
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

        public HierarchyNode TopLevelRecordingNode
        {
            get
            {
                HierarchyNode recordingNode = this.Recording != null ? this : null;
                var node = this;
                while (node.Level != 0)
                {
                    node = node.Parent;

                    if (node.Recording != null)
                        recordingNode = node;
                }

                return recordingNode;
            }
        }

        public void Update(Command command)
        {
            Command = command;
            Value = command;
        }

        public void Update(Recording recording)
        {
            Recording = recording;
            Value = recording;

            Children = new List<HierarchyNode>();

            foreach (var node in recording)
                AddChildRecursively(node);
        }

        public HierarchyNode GetNodeFromValue(Command command)
        {
            return GetAllNodes().FirstOrDefault(n => n.Value.Equals(command));
        }

        public HierarchyNode GetNode(Guid guid)
        {
            return GetAllNodes(true).FirstOrDefault(n =>
            {
                if (n.Value is IHaveGuid guidObj)
                    return guidObj.Guid == guid;

                return false;
            });
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

    public class HierarchyNodeDropDetails
    {
        public object Owner;
        public Action<HierarchyNode> DragAndDropAccepted;
        public BaseHierarchyManager HierarchyManager;
    }
}
