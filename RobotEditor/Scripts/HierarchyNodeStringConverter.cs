using Robot;
using Robot.Scripts;
using RobotRuntime;
using RobotRuntime.Abstractions;
using System;
using System.Text.RegularExpressions;
using RobotEditor.Abstractions;

namespace RobotEditor.Scripts
{
    public class HierarchyNodeStringConverter : IHierarchyNodeStringConverter
    {
        private const string RegexCoordinateRecognizeRules = @"\(\d+[, ]+\d+\)";
        private const string RegexGuidRecognizeRules = @"[\d\D]{8}-[\d\D]{4}-[\d\D]{4}-[\d\D]{4}-[\d\D]{12}";

        private IAssetGuidManager AssetGuidManager;
        public HierarchyNodeStringConverter(IAssetGuidManager AssetGuidManager)
        {
            this.AssetGuidManager = AssetGuidManager;
        }
        
        public string ToString(HierarchyNode node)
        {
            var s = node.Value.ToString();
            s = ReplaceCoordinatesIfTheyAreOverridenByParentNestedCommand(node, s);
            s = ReplaceImagePlaceholderToAnActualImageName(node, s);
            return s;
        }

        private string ReplaceCoordinatesIfTheyAreOverridenByParentNestedCommand(HierarchyNode node, string s)
        {
            if (node.Command != null && Regex.IsMatch(s, RegexCoordinateRecognizeRules))
            {
                if (node.Parent != null && node.Parent.Command != null && node.Parent.Command.CanBeNested())
                {
                    var assetGuidObj = CommandFactory.GetPropertyIfExist(node.Parent.Command, CommandFactory.k_Asset);
                    if (assetGuidObj != null)
                    {
                        var guid = (Guid)assetGuidObj;
                        var path = AssetGuidManager.GetPath(guid);
                        var assetName = ((path != "" && path != null) ? Commons.GetName(path) : "...");

                        s = Regex.Replace(s, RegexCoordinateRecognizeRules, "<" + assetName + ">");
                    }
                }
            }

            return s;
        }

        private string ReplaceImagePlaceholderToAnActualImageName(HierarchyNode node, string s)
        {
            if (node.Command != null && Regex.IsMatch(s, RegexGuidRecognizeRules))
            {
                var assetGuidObj = CommandFactory.GetPropertyIfExist(node.Command, CommandFactory.k_Asset);
                if (assetGuidObj != null)
                {
                    var guid = (Guid)assetGuidObj;
                    var path = AssetGuidManager.GetPath(guid);
                    var assetName = ((path != "" && path != null) ? Commons.GetName(path) : "...");
                    s = Regex.Replace(s, RegexGuidRecognizeRules, assetName);
                }
            }

            return s;
        }
    }
}
