﻿using Robot;
using Robot.Abstractions;
using RobotEditor.PropertyUtils;
using RobotEditor.Utils;
using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Utils.Win32;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace RobotEditor.Inspector
{
    public class NativeCommandProperties : CommandProperties
    {
        private IAssetManager AssetManager;
        private IAssetGuidManager AssetGuidManager;
        public NativeCommandProperties(IAssetManager AssetManager, IAssetGuidManager AssetGuidManager, ICommandFactory CommandFactory)
            : base(CommandFactory)
        {
            this.AssetManager = AssetManager;
            this.AssetGuidManager = AssetGuidManager;

            Properties = TypeDescriptor.GetProperties(this);
        }

        public override void HideProperties(ref DynamicTypeDescriptor dt)
        {
            dt.Properties.Clear();
            AddProperty(dt, "OverrideTitle");
            AddProperty(dt, "CommandType");

            if (Command is CommandDown || Command is CommandRelease || Command is CommandClick)
            {
                AddProperty(dt, "X");
                AddProperty(dt, "Y");
                AddProperty(dt, "DontMove");
            }
            if (Command is CommandDown || Command is CommandClick)
            {
                AddProperty(dt, "MouseButton");
            }
            else if (Command is CommandMove)
            {
                AddProperty(dt, "X");
                AddProperty(dt, "Y");
            }
            else if (Command is CommandForImage)
            {
                AddProperty(dt, "Asset");
                AddProperty(dt, "Timeout");
                AddProperty(dt, "DetectionMode");
                AddProperty(dt, "ForEach");
            }
            else if (Command is CommandSleep)
            {
                AddProperty(dt, "Time");
            }
            else if (Command is CommandRunRecording)
            {
                AddProperty(dt, "Recording");
            }
            else if (Command is CommandWriteText)
            {
                AddProperty(dt, "Text");
            }
            else if (Command is CommandPressKey)
            {
                AddProperty(dt, "KeyCode");
            }
            else if (Command is CommandForText)
            {
                AddProperty(dt, "Text");
                AddProperty(dt, "Timeout");
                AddProperty(dt, "TextDetectionMode");
                AddProperty(dt, "TextComparisonThreshold");
                AddProperty(dt, "ForEach");
            }
            if (Command is CommandIfImageVisible)
            {
                AddProperty(dt, "Asset");
                AddProperty(dt, "Timeout");
                AddProperty(dt, "ExpectTrue");
                AddProperty(dt, "DetectionMode");
            }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue("")]
        [DisplayName("Title")]
        public string OverrideTitle
        {
            get { return DynamicCast(Command).OverrideTitle; }
            set { DynamicCast(Command).OverrideTitle = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("X")]
        public int X
        {
            get { return DynamicCast(Command).X; }
            set { DynamicCast(Command).X = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("Y")]
        public int Y
        {
            get { return DynamicCast(Command).Y; }
            set { DynamicCast(Command).Y = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(false)]
        [DisplayName("Dont Move")]
        public bool DontMove
        {
            get { return DynamicCast(Command).DontMove; }
            set { DynamicCast(Command).DontMove = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("Time")]
        public int Time
        {
            get { return DynamicCast(Command).Time; }
            set { DynamicCast(Command).Time = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0)]
        [DisplayName("Timeout (ms)")]
        public int Timeout
        {
            get { return DynamicCast(Command).Timeout; }
            set { DynamicCast(Command).Timeout = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(false)]
        [DisplayName("Run for every match?")]
        public bool ForEach
        {
            get { return DynamicCast(Command).ForEach; }
            set { DynamicCast(Command).ForEach = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(false)]
        [DisplayName("Smooth")]
        public bool Smooth
        {
            get { return DynamicCast(Command).Smooth; }
            set { DynamicCast(Command).Smooth = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DisplayName("Referenced Asset")]
        [TypeConverter(typeof(AssetGUIDImageStringConverter))]
        [Editor(typeof(AssetGUIDImageUITypeEditor), typeof(UITypeEditor))]
        public string Asset
        {
            get
            {
                var guid = DynamicCast(Command).Asset;
                var path = AssetGuidManager.GetPath(guid);
                return path;
            }
            set
            {
                Asset asset = AssetManager.GetAsset(value);
                if (asset != null)
                    DynamicCast(Command).Asset = asset.Guid;
            }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DisplayName("Referenced Recording")]
        [TypeConverter(typeof(RecordingGUIDStringConverter))]
        public string Recording
        {
            get
            {
                var guid = DynamicCast(Command).Asset;
                var path = AssetGuidManager.GetPath(guid);
                return path;
            }
            set
            {
                Asset asset = AssetManager.GetAsset(value);
                if (asset != null)
                    DynamicCast(Command).Asset = asset.Guid;
            }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue("")]
        [DisplayName("Text")]
        public string Text
        {
            get { return DynamicCast(Command).Text; }
            set { DynamicCast(Command).Text = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue("")]
        [DisplayName("KeyCode")]
        public Keys KeyCode
        {
            get { return DynamicCast(Command).KeyCode; }
            set { DynamicCast(Command).KeyCode = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(MouseButton.Left)]
        [DisplayName("Mouse Button")]
        public MouseButton MouseButton
        {
            get { return DynamicCast(Command).MouseButton; }
            set { DynamicCast(Command).MouseButton = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(true)]
        [DisplayName("Expect True")]
        public bool ExpectTrue
        {
            get { return DynamicCast(Command).ExpectTrue; }
            set { DynamicCast(Command).ExpectTrue = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue("Default")]
        [DisplayName("Image Detection Mode")]
        [TypeConverter(typeof(DetectorNameStringConverterWithDefault))]
        public string DetectionMode
        {
            get { return DynamicCast(Command).DetectionMode; }
            set { DynamicCast(Command).DetectionMode = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue("Default")]
        [DisplayName("Text Detection Mode")]
        [TypeConverter(typeof(TextDetectorNameStringConverterWithDefault))]
        public string TextDetectionMode
        {
            get { return DynamicCast(Command).TextDetectionMode; }
            set { DynamicCast(Command).TextDetectionMode = value; }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(0.80f)]
        [DisplayName("Text Comparison Threshold")]
        public float TextComparisonThreshold
        {
            get { return DynamicCast(Command).TextComparisonThreshold; }
            set { DynamicCast(Command).TextComparisonThreshold = value; }
        }
    }
}
