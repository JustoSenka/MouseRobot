using RobotEditor.Utils;
using System.ComponentModel;

namespace RobotEditor.Settings
{
    public abstract class BaseProperties
    {
        [Browsable(false)]
        public virtual string Title { get { return "Properties"; } }

        public virtual void HideProperties(ref DynamicTypeDescriptor dt)
        {

        }
        public virtual void OnPropertiesModified()
        {

        }
    }
}
