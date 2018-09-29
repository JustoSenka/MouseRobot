using RobotRuntime.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace RobotRuntime
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns true if current method was called from that class or self.
        /// 
        /// Usage:
        ///    typeof(MyClass).IsTheCaller()
        ///    myClassInstance.IsTheCaller()
        ///    
        /// Cannot be used on static types.
        /// </summary>
        public static bool IsTheCaller<ExpectedCallerClass>(this ExpectedCallerClass expectedCaller) where ExpectedCallerClass : class
        {
            var checkerType = new StackFrame(1).GetMethod().DeclaringType;
            var actualCallerType = new StackFrame(2).GetMethod().DeclaringType;

            if (checkerType == actualCallerType)
                return true;

            if (expectedCaller is Type)
                return (expectedCaller as Type) == actualCallerType;

            return actualCallerType == typeof(ExpectedCallerClass);
        }

        public static void MoveAfter<T>(this IList<T> list, int indexToMove, int indexOfTarget)
        {
            var elem = list[indexToMove];
            list.RemoveAt(indexToMove);

            // If taken from beginning at putting to the end, index will be smaller, since after removal they all get shifted
            indexOfTarget += (indexToMove <= indexOfTarget) ? 0 : 1;
            list.Insert(indexOfTarget, elem);
        }

        public static void MoveBefore<T>(this IList<T> list, int indexToMove, int indexOfTarget)
        {
            list.MoveAfter(indexToMove, indexOfTarget - 1);
        }

        public static int IndexOf<T, K>(this T collection, K element) where T : IReadOnlyCollection<K> where K : class
        {
            var index = collection.TakeWhile((s) => s != element).Count();
            if (collection.ElementAt(index) != element)
                throw new Exception("Index not found for: " + element.ToString());
            return index;
        }

        public static IEnumerable<K> CastAndRemoveNullsTree<K>(this TreeNode<Command> tree) where K : class
        {
            foreach (var node in tree.GetAllNodes())
            {
                var casted = node.value as K;
                if (casted != null)
                    yield return casted;
            }
        }

        public static IEnumerable<K> CastAndRemoveNulls<K, T>(this IEnumerable<T> collection) where K : class where T : class
        {
            foreach (var node in collection)
            {
                var casted = node as K;
                if (casted != null)
                    yield return casted;
            }
        }

        public static LinkedListNode<T> NodeAt<T>(this LinkedList<T> list, int index)
        {
            var currentNode = list.First;

            while (true)
            {
                if (--index == -1)
                    return currentNode;

                currentNode = currentNode.Next;
                if (currentNode == null)
                    return null;
            }
        }

        public static int IndexOf<T>(this LinkedList<T> list, T item)
        {
            var count = 0;
            for (var node = list.First; node != null; node = node.Next, count++)
            {
                if (item.Equals(node.Value))
                    return count;
            }
            return -1;
        }

        public static void ForEach<T, K>(this T source, Action<K> action) where T : IEnumerable where K : class
        {
            foreach (var item in source)
                action(item as K);
        }

        public static int Count(this IEnumerable enumarable)
        {
            int result = 0;
            var enumerator = enumarable.GetEnumerator();

            while (enumerator.MoveNext())
                result++;

            return result;
        }

        public static float Clamp(this float source, float min, float max)
        {
            source = source > min ? source : min;
            source = source < max ? source : max;
            return source;
        }

        public static TreeNode FindChildRegex(this TreeView treeView, string regex)
        {
            return treeView.Nodes.Cast<TreeNode>().Where(r => Regex.IsMatch(r.Text, regex)).FirstOrDefault();
        }

        public static TreeNode FindChildRegex(this TreeNode treeView, string regex)
        {
            return treeView.Nodes.Cast<TreeNode>().Where(r => Regex.IsMatch(r.Text, regex)).FirstOrDefault();
        }

        public static TreeNode FindChild(this TreeView treeView, string name)
        {
            return treeView.Nodes.Cast<TreeNode>().Where(r => r.Text == name).FirstOrDefault();
        }

        public static TreeNode FindChild(this TreeNode treeView, string name)
        {
            return treeView.Nodes.Cast<TreeNode>().Where(r => r.Text == name).FirstOrDefault();
        }

        public static TreeNode FindNode(this TreeView treeView, string path)
        {
            var folderNode = treeView.FindChild(Paths.GetFolder(path));
            return folderNode.FindChild(Paths.GetName(path));
        }

        public static bool IsDefault<T>(this T value) where T : struct
        {
            return value.Equals(default(T));
        }

        public static void Post(this AsyncOperation async, Action ac)
        {
            async?.Post(new SendOrPostCallback(delegate (object state)
            {
                ac.Invoke();
            }), null);
        }
    }
}
