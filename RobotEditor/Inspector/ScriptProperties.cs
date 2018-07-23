using Robot;
using Robot.Abstractions;
using Robot.Scripts;
using RobotEditor.Settings;
using RobotEditor.Utils;
using RobotRuntime.Scripts;
using RobotRuntime.Tests;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

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

            var isSpecialScript = IsSpecialScript(Script, BaseScriptManager);

            if (!isSpecialScript)
                AddProperty(dt, "Name");
            else
                AddProperty(dt, "ReadonlyName");
        }

        private bool IsSpecialScript(Script Script, BaseScriptManager BaseScriptManager)
        {
            return LightTestFixture.IsSpecialScript(Script) ||
                BaseScriptManager is ScriptManager;
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
                if (BaseScriptManager.LoadedScripts.Any(Script => Script.Name == value))
                {
                    FlexibleMessageBox.Show("Property value is not valid.\nTest with this name already exists: " + value, 
                        "Inspector Window", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Script.Name = value;
            }
        }

        [SortedCategory("Test Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DisplayName("Test Name")]
        public string ReadonlyName
        {
            get
            {
                return Script.Name;
            }
        }

        protected void AddProperty(DynamicTypeDescriptor dt, string name)
        {
            dt.AddProperty(Properties.Find(name, false));
        }
    }
}
