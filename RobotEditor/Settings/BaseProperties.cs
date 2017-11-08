using RobotEditor.Utils;
using System;
using System.ComponentModel;

namespace RobotEditor.Settings
{
    public class BaseProperties
    {
        [Browsable(false)]
        public virtual string Title { get { return "Properties"; } }
        public virtual void HideProperties(DynamicTypeDescriptor dt)
        {

        }
    }
}
