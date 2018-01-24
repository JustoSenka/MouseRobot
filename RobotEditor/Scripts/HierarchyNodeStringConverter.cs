using Robot;
using Robot.Scripts;
using RobotRuntime;
using RobotRuntime.Assets;
using System;
using System.Text.RegularExpressions;

namespace RobotEditor.Scripts
{
    internal class HierarchyNodeStringConverter
    {
        private HierarchyNode m_Node;
        private const string RegexCoordinateRecognizeRules = @"\(\d+[, ]+\d+\)";

        internal HierarchyNodeStringConverter(HierarchyNode node)
        {
            m_Node = node;
        }

        internal string Str { get { return ToString(m_Node); } }

        internal static string ToString(HierarchyNode node)
        {
            var s = node.Value.ToString();
            s = ReplaceCoordinatesIfTheyAreOverridenByParentNestedCommand(node, s);
            return s;
        }

        private static string ReplaceCoordinatesIfTheyAreOverridenByParentNestedCommand(HierarchyNode node, string s)
        {
            if (node.Command != null && Regex.IsMatch(s, RegexCoordinateRecognizeRules))
            {
                if (node.Parent != null && node.Parent.Command != null && node.Parent.Command.CanBeNested())
                {
                    var assetGuidObj = CommandFactory.GetPropertyIfExist(node.Parent.Command, CommandFactory.k_Asset);
                    if (assetGuidObj != null)
                    {
                        var guid = (Guid)assetGuidObj;
                        var path = AssetGuidManager.Instance.GetPath(guid);
                        var assetName = ((path != "" && path != null) ? Commons.GetName(path) : "...");

                        s = Regex.Replace(s, RegexCoordinateRecognizeRules, "<" + assetName + ">");
                    }
                }
            }

            return s;
        }
    }
}
