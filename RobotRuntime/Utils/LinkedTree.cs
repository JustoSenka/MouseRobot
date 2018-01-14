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

        public void Join(TreeNode<T> anotherTree)
        {
            children.AddLast(anotherTree);
            anotherTree.parent = this;
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
            if (index == 0 || children.Count == 0)
            {
                children.AddFirst(child);
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

        public bool Remove(T value)
        {
            return Remove(GetNodeFromValue(value));
        }

        public bool Remove(TreeNode<T> node)
        {
            return node.parent.children.Remove(node);
        }

        public void RemoveAt(int index)
        {
            children.Remove(children.NodeAt(index));
        }

        public void MoveAfter(TreeNode<T> source, TreeNode<T> dest)
        {
            if (source == dest)
                return;

            var destLinkedListNode = dest.parent.children.Find(dest);

            source.parent.Remove(source);

            dest.parent.children.AddAfter(destLinkedListNode, source);
            source.parent = dest.parent;
        }

        public void MoveBefore(TreeNode<T> source, TreeNode<T> dest)
        {
            if (source == dest)
                return;

            var destLinkedListNode = dest.parent.children.Find(dest);

            source.parent.Remove(source);

            dest.parent.children.AddBefore(destLinkedListNode, source);
            source.parent = dest.parent;
        }

        public void MoveAfter(int source, int dest)
        {
            if (source == dest)
                return;

            var sourceNode = GetChild(source);
            var destNode = GetChild(dest);

            if (dest == -1)
                MoveBefore(sourceNode, GetChild(0));
            else
                MoveAfter(sourceNode, destNode);
        }

        public void MoveBefore(int source, int dest)
        {
            if (source == dest)
                return;

            var sourceNode = GetChild(source);
            var destNode = GetChild(dest);
            MoveBefore(sourceNode, destNode);
        }

        public void MoveAfter(T source, T dest)
        {
            if (source.Equals(dest))
                return;

            var sourceNode = GetNodeFromValue(source);
            var destNode = GetNodeFromValue(dest);
            MoveAfter(sourceNode, destNode);
        }

        public void MoveBefore(T source, T dest)
        {
            if (source.Equals(dest))
                return;

            var sourceNode = GetNodeFromValue(source);
            var destNode = GetNodeFromValue(dest);
            MoveBefore(sourceNode, destNode);
        }

        public TreeNode<T> GetChild(int i)
        {
            foreach (TreeNode<T> n in children)
                if (--i == -1)
                    return n;
            return null;
        }

        public int Index
        {
            get
            {
                if (parent == null)
                    return 0;

                return parent.children.IndexOf(this);
            }
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
                var childClone = (TreeNode<T>)node.Clone();
                childClone.parent = clone;
                clone.children.AddLast(childClone);
            }
            return clone;
        }
    }
}
