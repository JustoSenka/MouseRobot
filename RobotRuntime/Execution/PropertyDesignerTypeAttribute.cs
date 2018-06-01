using System;

namespace RobotRuntime.Execution
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PropertyDesignerTypeAttribute : Attribute
    {
        public PropertyDesignerTypeAttribute(string typeName) : base()
        {
            this.typeName = typeName;
        }

        public string typeName;
    }
}
