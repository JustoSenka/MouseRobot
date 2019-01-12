using Robot.Abstractions;
using RobotEditor.Inspector;
using RobotEditor.Utils;
using System.ComponentModel;

namespace RobotEditor.Resources.ScriptTemplates
{
    public class CustomCommandDesigner : CommandProperties
    {
        public CustomCommandDesigner(ICommandFactory CommandFactory)
            : base(CommandFactory)
        {
            Properties = TypeDescriptor.GetProperties(this);
        }

        public override void HideProperties(ref DynamicTypeDescriptor dt)
        {
            dt.Properties.Clear();
            AddProperty(dt, "CommandType");
            AddProperty(dt, "SomeInt");
        }

        [SortedCategory("Command Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DefaultValue(5)]
        [DisplayName("Some Int")]
        public int SomeInt
        {
            get { return (Command as CustomCommand).SomeInt; }
            set { (Command as CustomCommand).SomeInt = value; }
        }
    }
}
