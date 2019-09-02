using Robot;
using Robot.Abstractions;
using Robot.Recordings;
using Robot.Tests;
using RobotEditor.Settings;
using RobotEditor.Utils;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace RobotEditor.Inspector
{
    public class RecordingProperties : BaseProperties
    {
        [Browsable(false)]
        public virtual Recording Recording { get; set; }

        [Browsable(false)]
        public override string Title { get { return "Test Properties"; } }

        protected PropertyDescriptorCollection Properties;
        private ICommandFactory CommandFactory;
        public RecordingProperties(ICommandFactory CommandFactory)
        {
            this.CommandFactory = CommandFactory;

            Properties = TypeDescriptor.GetProperties(this);
        }

        protected const int NumOfCategories = 1;
        protected const int CommandPropertiesCategoryPosition = 1;

        public override void HideProperties(ref DynamicTypeDescriptor dt)
        {
            dt.Properties.Clear();

            var canBeRenamed = CanBeRenamed(Recording, BaseHierarchyManager);

            if (canBeRenamed)
                AddProperty(dt, "Name");
            else
                AddProperty(dt, "ReadonlyName");
        }

        private bool CanBeRenamed(Recording Recording, IBaseHierarchyManager BaseHierarchyManager)
        {
            return BaseHierarchyManager is TestFixture && !LightTestFixture.IsSpecialRecording(Recording) || // Non special recordings in TestFixtureWindow
                BaseHierarchyManager is HierarchyManager && Recording.Path.IsEmpty(); // Non saved recordings in HierarchyWindow
        }

        [SortedCategory("Test Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DisplayName("Test Name")]
        public string Name
        {
            get
            {
                return Recording.Name;
            }
            set
            {
                if (BaseHierarchyManager.LoadedRecordings.Any(Recording => Recording.Name == value))
                {
                    FlexibleMessageBox.Show("Property value is not valid.\nTest with this name already exists: " + value, 
                        "Inspector Window", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Recording.Name = value;
            }
        }

        [SortedCategory("Test Properties", CommandPropertiesCategoryPosition, NumOfCategories)]
        [DisplayName("Test Name")]
        public string ReadonlyName
        {
            get
            {
                return Recording.Name;
            }
        }

        protected void AddProperty(DynamicTypeDescriptor dt, string name)
        {
            dt.AddProperty(Properties.Find(name, false));
        }
    }
}
