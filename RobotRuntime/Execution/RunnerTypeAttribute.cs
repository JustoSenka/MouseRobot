using System;

namespace RobotRuntime.Execution
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class RunnerTypeAttribute : Attribute
    {
        public RunnerTypeAttribute(Type type) : base()
        {
            this.type = type;
        }

        public Type type;
    }
}
