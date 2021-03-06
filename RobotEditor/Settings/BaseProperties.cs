﻿using Robot.Abstractions;
using RobotEditor.Utils;
using RobotRuntime.Settings;
using System;
using System.ComponentModel;

namespace RobotEditor.Settings
{
    public abstract class BaseProperties
    {
        [Browsable(false)]
        public virtual string Title { get { return "Properties"; } }

        [Browsable(false)]
        public virtual string HelpTextTitle { get; }

        [Browsable(false)]
        public virtual string HelpTextContent { get; }

        [Browsable(false)]
        [NonSerialized]
        public IBaseHierarchyManager BaseHierarchyManager;

        [Browsable(false)]
        public BaseSettings Settings;

        public BaseProperties(BaseSettings Settings)
        {
            this.Settings = Settings;
        }

        public virtual void HideProperties(ref DynamicTypeDescriptor dt)
        {

        }
        public virtual void OnPropertiesModified()
        {

        }
    }
}
