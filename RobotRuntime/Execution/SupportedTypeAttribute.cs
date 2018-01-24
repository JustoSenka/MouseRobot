using System;

namespace RobotRuntime.Execution
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class SupportedTypeAttribute : Attribute
    {
        public SupportedTypeAttribute(Type type) : base()
        {
            this.type = type;
        }

        public Type type;
    }
}
