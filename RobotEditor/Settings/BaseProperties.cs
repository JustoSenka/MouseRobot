using RobotEditor.Utils;
using RobotRuntime.Settings;
using System;
using System.ComponentModel;

namespace RobotEditor.Scripts
{
    public abstract class BaseProperties
    {
        [Browsable(false)]
        public virtual string Title { get { return "Properties"; } }

        public virtual void HideProperties(DynamicTypeDescriptor dt)
        {

        }
        public virtual void OnPropertiesModified()
        {

        }
    }
}
