using RobotRuntime.Utils;
using System;
using System.ComponentModel;

namespace RobotRuntime.Settings
{
    public abstract class BaseSettings
    {
        [Browsable(false)]
        public virtual string FriendlyName => Paths.SplitCamelCase(this.GetType().Name);
    }
}
