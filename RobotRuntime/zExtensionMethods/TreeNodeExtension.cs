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
        /// <summary>
        /// Will find a child node by value.ToString. Iterates only direct children
        /// Returns null if not found
        /// </summary>
        public static TreeNode<T> FindChild<T>(this TreeNode<T> treeNode, string name)
        {
            return treeNode.FirstOrDefault(n => n.value.ToString() == name);
        }

        /// <summary>
        /// Will find a child node by value.ToString using regex. Iterates only direct children
        /// Returns null if not found
        /// </summary>
        public static TreeNode<T> FindChildRegex<T>(this TreeNode<T> treeNode, string regex)
        {
            return treeNode.FirstOrDefault(n => Regex.IsMatch(n.ToString(), regex));
        }

        /// <summary>
        /// Will split path into directory elements and will call FindChild recursivelly with all elements.
        /// Ignores file extension.
        /// Example: "Assets/scripts/sc.mrb" => new [] { "Assets", "scripts", "sc.mrb" }
        /// Will look for node:
        ///  - Assets
        ///  -- scripts
        ///  --- sc
        /// </summary>
        public static TreeNode<T> FindNodeFromPath<T>(this TreeNode<T> treeNode, string path)
        {
            var dirs = Paths.GetPathDirectoryElementsWtihFileName(path);
            return treeNode.FindNodeFromPath(dirs);
        }

        /// <summary>
        /// Will call FindChild recursivelly with all path elements
        /// Example: from new [] { "Assets", "scripts", "sc.mrb" }
        /// Will look for node:
        ///  - Assets
        ///  -- scripts
        ///  --- sc
        /// </summary>
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
                return null;
            }

            return currentNode;
        }

        /// <summary>
        /// Will travers tree to correct node and add child there with specified name in the path.
        /// Example: With "Assets/scripts/folderA" it will traverse to "folderA" node and add new child of "objValue"
        /// </summary>
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

        /// <summary>
        /// From tree node will construct a path to reach it.
        /// </summary>
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
