using RobotEditor.Hierarchy;
using System.Collections.Generic;

namespace RobotEditor
{
    public static class HierarchyNodeExtension
    {
        public static void ReplaceNodeWithNewOne(this IList<HierarchyNode> nodeList, HierarchyNode newNode)
        {
            foreach (var n in nodeList)
            {
                if (n.Value == newNode.Value)
                {
                    if (newNode.Recording != null)
                        n.Update(newNode.Recording);
                    else if (newNode.Command != null)
                        n.Update(newNode.Command);
                }

                if (n.Children != null && n.Children.Count > 0)
                    n.Children.ReplaceNodeWithNewOne(newNode);
            }
        }

        public static HierarchyNode FindRecursively(this IList<HierarchyNode> nodeList, object value)
        {
            foreach (var n in nodeList)
            {
                if (n.Value == value)
                    return n;

                if (n.Children != null && n.Children.Count > 0)
                {
                    var node =  n.Children.FindRecursively(value);
                    if (node != null)
                        return node;
                }
            }

            return null;
        }

        /// <summary>
        /// This is specific only for TestFixtureWindow and TestFixture, since 4 initial recordings are in separate group.
        /// </summary>
        public static void RemoveAtIndexRemoving4(this IList<HierarchyNode> nodeList, int index)
        {
            if (index <= 3)
                nodeList[0].Children.RemoveAt(index);
            else
                nodeList[1].Children.RemoveAt(index - 4);
        }
    }
}
