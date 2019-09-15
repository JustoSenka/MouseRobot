using RobotRuntime.Utils;

namespace RobotRuntime.Settings
{
    public abstract class BaseSettings
    {
        public virtual string FriendlyName => Paths.SplitCamelCase(this.GetType().Name);
    }
}
