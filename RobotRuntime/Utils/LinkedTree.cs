using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RobotRuntime.Utils
{
    public delegate void TreeVisitor<T>(T nodeData);

    [Serializable]
    public class TreeNode<T> : IEnumerable<TreeNode<T>>, ICloneable
    {
        public T value;
        public TreeNode<T> parent;
        private LinkedList<TreeNode<T>> children;

        public TreeNode()
        {
            children = new LinkedList<TreeNode<T>>();
        }

        public TreeNode(T value)
        {
            this.value = value;
            children = new LinkedList<TreeNode<T>>();
        }

        public TreeNode<T> AddChild(T value)
        {
            var child = new TreeNode<T>(value);
            child.parent = this;
            children.AddLast(child);
            return child;
        }

        public TreeNode<T> Insert(int index, T value)
        {
            var child = new TreeNode<T>(value);
            child.parent = this;

            LinkedListNode<TreeNode<T>> node;
            if (index == 0)
            {
                node = children.First;
                children.AddBefore(node, child);
            }
            else
            {
                node = children.NodeAt(index - 1);
                children.AddAfter(node, child);
            }

            return child;
        }

        public int IndexOf(T value)
        {
            int i = 0;
            foreach (var node in children)
            {
                if (value.Equals(node.value))
                    return i;
                ++i;
            }
            return -1;
        }

        public void RemoveAt(int index)
        {
            children.Remove(children.NodeAt(index));
        }

        public void MoveAfter(int source, int dest)
        {
            if (source == dest)
                return;

            var sourceNode = children.NodeAt(source);
            var destNode = children.NodeAt(dest);
            children.Remove(sourceNode);

            if (dest == -1)
                children.AddBefore(children.First, sourceNode);
            else
                children.AddAfter(destNode, sourceNode);
        }

        public void MoveBefore(int source, int dest)
        {
            if (source == dest)
                return;

            var sourceNode = children.NodeAt(source);
            var destNode = children.NodeAt(dest);
            children.Remove(sourceNode);
            children.AddBefore(destNode, sourceNode);
        }

        public TreeNode<T> GetChild(int i)
        {
            foreach (TreeNode<T> n in children)
                if (--i == -1)
                    return n;
            return null;
        }

        /// <summary>
        /// Checks all nodes and child child nodes for correct value and returns that node
        /// </summary>
        public TreeNode<T> GetNodeFromValue(T value)
        {
            var all = GetAllNodes(false);
            return all.FirstOrDefault(n => n.value.Equals(value));
        }

        public void Traverse(TreeNode<T> node, TreeVisitor<T> visitor)
        {
            visitor(node.value);
            foreach (TreeNode<T> child in node.children)
                Traverse(child, visitor);
        }

        /// <summary>
        /// Returns all nodes in the tree hierarchy recursivelly, including all child and grandchild nodes
        /// </summary>
        public IEnumerable<TreeNode<T>> GetAllNodes(bool includeSelf = true)
        {
            if (includeSelf)
                yield return this;

            foreach (var c in children)
            {
                yield return c;
                foreach (var child in c.GetAllNodes(false))
                    yield return child;
            }
        }

        public IEnumerator<TreeNode<T>> GetEnumerator()
        {
            return children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public object Clone()
        {
            var clone = value is ICloneable ? new TreeNode<T>((T)(value as ICloneable).Clone()) : new TreeNode<T>(value);
            clone.children = new LinkedList<TreeNode<T>>();
            clone.parent = parent;
            foreach (var node in children)
            {
                clone.children.AddLast((TreeNode<T>)node.Clone());
            }
            return clone;
        }
    }
}
