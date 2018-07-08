using Robot.Abstractions;
using RobotEditor.PropertyUtils;
using RobotEditor.Settings;
using RobotEditor.Utils;
using RobotRuntime.Scripts;
using System.ComponentModel;

namespace RobotEditor.Inspector
{
    public class ScriptProperties : BaseProperties
    {
        [Browsable(false)]
        public virtual Script Script { get; set; }

        [Browsable(false)]
        public override string Title { get { return "Test Properties"; } }

        protected PropertyDescriptorCollection Properties;
        private ICommandFactory CommandFactory;
        public ScriptProperties(ICommandFactory CommandFactory)
        {
            this.CommandFactory = CommandFactory;

            Properties = TypeDescriptor.GetProperties(this);
        }

        protected const int NumOfCategories = 1;
        protected const int CommandPropertiesCategoryPosition = 1;

        public override void HideProperties(ref DynamicTypeDescriptor dt)
        {
            dt.Properties.Clear();
            AddProperty(dt, "Name");
        }

        [SortedCategory("Test Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DisplayName("Test Name")]
        public string Name
        {
            get
            {
                return Script.Name;
            }
            set
            {
                Script.Name = value;
            }
        }

        protected void AddProperty(DynamicTypeDescriptor dt, string name)
        {
            dt.AddProperty(Properties.Find(name, false));
        }
    }
}
