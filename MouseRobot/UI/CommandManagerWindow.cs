using Robot;
using RobotUI.Utils;
using System.ComponentModel;
using System.Reflection;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotUI
{
    public partial class CommandManagerWindow : DockContent
    {
        public CommandManagerWindow()
        {
            MouseRobot.Instance.commandManagerProperties = new CommandManagerProperties();

            InitializeComponent();

            propertyGrid.SelectedObject = MouseRobot.Instance.commandManagerProperties;
        }

        private void propertyGrid_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            DynamicTypeDescriptor dt = new DynamicTypeDescriptor(typeof(CommandManagerProperties));

            if (MouseRobot.Instance.commandManagerProperties.TreatMouseDownAsMouseClick)
                dt.RemoveProperty("ThresholdBetweenMouseDownAndMouseUp");

            propertyGrid.SelectedObject = dt.FromComponent(MouseRobot.Instance.commandManagerProperties);
        }
    }
}
