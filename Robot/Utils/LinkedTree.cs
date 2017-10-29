using System.Collections;
using System.Collections.Generic;

namespace Robot.Utils
{
    public delegate void TreeVisitor<T>(T nodeData);

    public class TreeNode<T> : IEnumerable<TreeNode<T>>
    {
        public T value;
        private LinkedList<TreeNode<T>> children;

        public TreeNode(T value)
        {
            this.value = value;
            children = new LinkedList<TreeNode<T>>();
        }

        public TreeNode<T> AddChild(T value)
        {
            var child = new TreeNode<T>(value);
            children.AddLast(child);
            return child;
        }

        public TreeNode<T> GetChild(int i)
        {
            foreach (TreeNode<T> n in children)
                if (--i == 0)
                    return n;
            return null;
        }

        public void Traverse(TreeNode<T> node, TreeVisitor<T> visitor)
        {
            visitor(node.value);
            foreach (TreeNode<T> child in node.children)
                Traverse(child, visitor);
        }

        public IEnumerator<TreeNode<T>> GetEnumerator()
        {
            return children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
