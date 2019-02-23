using RobotRuntime.Utils;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RobotRuntime
{
    public static class TreeNodeExtension
    {
        public static TreeNode<T> FindChild<T>(this TreeNode<T> treeNode, string name)
        {
            return treeNode.FirstOrDefault(n => n.value.ToString() == name);
        }

        public static TreeNode<T> FindChildRegex<T>(this TreeNode<T> treeNode, string regex)
        {
            return treeNode.FirstOrDefault(n => Regex.IsMatch(n.ToString(), regex));
        }

        public static TreeNode<T> FindNodeFromPath<T>(this TreeNode<T> treeNode, string path)
        {
            var dirs = Paths.GetPathDirectoryElementsWtihFileName(path);
            return treeNode.FindNodeFromPath(dirs);
        }

        public static TreeNode<T> FindNodeFromPath<T>(this TreeNode<T> treeNode, string[] pathElements)
        {
            var currentNode = treeNode;

            try
            {
                foreach (var dir in pathElements)
                    currentNode = currentNode.FindChild(Path.GetFileNameWithoutExtension(dir));
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Cannot find path in treeView: " + string.Join("\\", pathElements), e.Message);
            }

            return currentNode;
        }

        public static TreeNode<T> AddChildAtPath<T>(this TreeNode<T> treeNode, string path, T objValue)
        {
            var dirs = Paths.GetPathDirectoryElementsWtihFileName(path);

            TreeNode<T> nodeToAddChild = treeNode;
            var dirCount = dirs.Count();
            if (dirCount > 1)
            {
                var pathBefore = string.Join(Path.DirectorySeparatorChar.ToString(), dirs.Take(dirCount - 1).ToArray());
                nodeToAddChild = treeNode.FindNodeFromPath(pathBefore);
            }

            return nodeToAddChild.AddChild(objValue);
        }

        public static string FindNodePath<T>(this TreeNode<T> treeNode)
        {
            var builder = new StringBuilder();
            do
            {
                builder.Insert(0, treeNode.ToString() + Path.DirectorySeparatorChar);
                treeNode = treeNode.parent;
            }
            while (treeNode != null);

            return Paths.NormalizePath(builder.ToString());
        }
    }
}
