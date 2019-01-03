﻿using Robot;
using Robot.Abstractions;
using RobotEditor.PropertyUtils;
using RobotEditor.Utils;
using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Utils;
using RobotRuntime.Utils.Win32;
using System.ComponentModel;
using System.Drawing.Design;

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
            AddProperty(dt, "CommandType");

            if (Command is CommandDown || Command is CommandRelease || Command is CommandPress)
            {
                AddProperty(dt, "X");
                AddProperty(dt, "Y");
                AddProperty(dt, "DontMove");
            }
            if (Command is CommandDown || Command is CommandPress)
            {
                AddProperty(dt, "MouseButton");
            }
            else if (Command is CommandMove)
            {
                AddProperty(dt, "X");
                AddProperty(dt, "Y");
            }
            else if (Command is CommandForImage || Command is CommandForeachImage || Command is CommandIfImageVisible)
            {
                AddProperty(dt, "Asset");
                AddProperty(dt, "Timeout");
            }
            else if (Command is CommandSleep)
            {
                AddProperty(dt, "Time");
            }
            else if (Command is CommandRunRecording)
            {
                AddProperty(dt, "Script");
            }
            else if (Command is CommandWriteText)
            {
                AddProperty(dt, "Text");
            }
            if (Command is CommandIfImageVisible)
            {
                AddProperty(dt, "ExpectTrue");
            }
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
        [DisplayName("Timeout")]
        public int Timeout
        {
            get { return DynamicCast(Command).Timeout; }
            set { DynamicCast(Command).Timeout = value; }
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
                return (path == null || path == "") ? "..." : Paths.GetName(path);
            }
            set
            {
                Asset asset = AssetManager.GetAsset(Paths.ImageFolder, value);
                if (asset != null)
                    DynamicCast(Command).Asset = asset.Guid;
            }
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DisplayName("Referenced Script")]
        [TypeConverter(typeof(RecordingGUIDStringConverter))]
        public string Script
        {
            get
            {
                var guid = DynamicCast(Command).Asset;
                var path = AssetGuidManager.GetPath(guid);
                return (path == null || path == "") ? "..." : Paths.GetName(path);
            }
            set
            {
                Asset asset = AssetManager.GetAsset(Paths.ScriptFolder, value);
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
    }
}
