using RobotRuntime.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace RobotRuntime
{
    namespace Reflection
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

            public static int IndexOf<T, K>(this T collection, K element) where T : IReadOnlyCollection<K> where K : class
            {
                var index = collection.TakeWhile((s) => s != element).Count();
                if (collection.ElementAt(index) != element)
                    throw new Exception("Index not found for: " + element.ToString());
                return index;
            }
        }
    }

    public static class ExtensionMethods
    {
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

        public static IEnumerable<T> SafeCast<T>(this IEnumerable list) where T : class
        {
            return list.Cast<object>().Select(o => o as T);
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
            return treeView.Nodes.Cast<TreeNode>().FirstOrDefault(r => Regex.IsMatch(r.Text, regex));
        }

        public static TreeNode FindChildRegex(this TreeNode treeView, string regex)
        {
            return treeView.Nodes.Cast<TreeNode>().FirstOrDefault(r => Regex.IsMatch(r.Text, regex));
        }

        public static TreeNode FindChild(this TreeView treeView, string name)
        {
            return treeView.Nodes.Cast<TreeNode>().FirstOrDefault(r => r.Text == name);
        }

        public static TreeNode FindChild(this TreeNode treeView, string name)
        {
            return treeView.Nodes.Cast<TreeNode>().FirstOrDefault(r => r.Text == name);
        }

        public static TreeNode FindNode(this TreeView treeView, string path)
        {
            var dirs = Paths.GetPathDirectoryElementsWtihFileName(path);
            TreeNode currentNode = treeView.FindChild(dirs[0]);

            try
            {
                foreach (var dir in dirs.Skip(1))
                    currentNode = currentNode.FindChild(dir);
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Cannot find path in treeView: " + path, e.Message);
            }

            return currentNode;
        }

        public static string FindNodePath(this TreeNode treeNode)
        {
            var builder = new StringBuilder();
            do
            {
                builder.Insert(0, treeNode.Name + Path.DirectorySeparatorChar);
                treeNode = treeNode.Parent;
            }
            while (treeNode != null);

            return Paths.NormalizePath(builder.ToString());
        }

        public static void Post(this AsyncOperation async, Action ac)
        {
            async?.Post(new SendOrPostCallback(delegate (object state)
            {
                ac.Invoke();
            }), null);
        }

        public static string FixLineEndings(this string str)
        {
            return Regex.Replace(str, @"\r\n|\n\r|\n|\r", Environment.NewLine);
            // return Regex.Replace(str, $"{Environment.NewLine} +| +{Environment.NewLine}", Environment.NewLine).Trim();
        }

        public static string NormalizePath(this string str)
        {
            return Paths.NormalizePath(str);
        }

        public static string Quated(this string str)
        {
            return "\"" + str + "\"";
        }

        public static bool IsSubDirectoryOf(this string candidate, string other)
        {
            var isChild = false;
            try
            {
                var candidateInfo = new DirectoryInfo(candidate);
                var otherInfo = new DirectoryInfo(other);

                while (candidateInfo.Parent != null)
                {
                    if (candidateInfo.Parent.FullName == otherInfo.FullName)
                    {
                        isChild = true;
                        break;
                    }
                    else candidateInfo = candidateInfo.Parent;
                }
            }
            catch (Exception e)
            {
                var message = String.Format("Unable to check directories {0} and {1}: {2}", candidate, other, e);
                Logger.Log(LogType.Error, message);
            }

            return isChild;
        }
    }
}

public static class GlobalExtensionMethods
{
    public static bool IsDefault<T>(this T value)
    {
        return value == default;
    }

    public static bool IsEmpty(this string str)
    {
        return str == null || str == "" || str == string.Empty;
    }
}
