﻿using Robot.Recordings;
using RobotRuntime.Abstractions;
using System;
using System.Text.RegularExpressions;
using RobotEditor.Abstractions;
using RobotRuntime.Utils;
using RobotRuntime;
using Unity.Lifetime;

namespace RobotEditor.Hierarchy
{
    [RegisterTypeToContainer(typeof(IHierarchyNodeStringConverter), typeof(ContainerControlledLifetimeManager))]
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
                if (node.Parent != null && node.Parent.Command != null && node.Parent.Command.CanBeNested)
                {
                    // Replace coords with image asset name
                    var assetGuidObj = node.Parent.Command.GetPropertyIfExist(CommandFactory.k_Asset);
                    if (assetGuidObj != null)
                    {
                        var guid = (Guid)assetGuidObj;
                        var path = AssetGuidManager.GetPath(guid);
                        var assetName = ((path != "" && path != null) ? Paths.GetName(path) : "...");

                        s = Regex.Replace(s, RegexCoordinateRecognizeRules, "<" + assetName + ">");
                    }

                    // Replace coords with text on where to click
                    var text = node.Parent.Command.GetPropertyIfExist(CommandFactory.k_Text);
                    if (text != null)
                        s = Regex.Replace(s, RegexCoordinateRecognizeRules, "<" + text + ">");
                }
            }

            return s;
        }

        private string ReplaceImagePlaceholderToAnActualImageName(HierarchyNode node, string s)
        {
            if (node.Command != null && Regex.IsMatch(s, RegexGuidRecognizeRules))
            {
                var assetGuidObj = node.Command.GetPropertyIfExist(CommandFactory.k_Asset);
                if (assetGuidObj != null)
                {
                    var guid = (Guid)assetGuidObj;
                    var path = AssetGuidManager.GetPath(guid);
                    var assetName = ((path != "" && path != null) ? Paths.GetName(path) : "...");
                    s = Regex.Replace(s, RegexGuidRecognizeRules, assetName);
                }
            }

            return s;
        }
    }
}
