using System;

namespace RobotRuntime
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PropertyDesignerTypeAttribute : Attribute
    {
        public PropertyDesignerTypeAttribute(Type type) : base()
        {
            this.typeName = type.Name;
        }

        public PropertyDesignerTypeAttribute(string typeName) : base()
        {
            this.typeName = typeName;
        }

        public string typeName;
    }
}
