using Robot.Recordings;
using RobotEditor.Utils;
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
        public BaseHierarchyManager BaseHierarchyManager;

        public virtual void HideProperties(ref DynamicTypeDescriptor dt)
        {

        }
        public virtual void OnPropertiesModified()
        {

        }
    }
}
