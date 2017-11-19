using Robot;
using RobotEditor.Scripts;
using RobotEditor.Settings;
using RobotEditor.Utils;
using RobotRuntime;
using RobotRuntime.Commands;
using System;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor.Windows
{
    public partial class InspectorWindow : DockContent
    {
        private CommandProperties<Command> m_CurrentObject;

        public InspectorWindow()
        {
            InitializeComponent();
            propertyGrid.SelectedObject = null;
        }

        public void ShowCommand<T>(T command) where T : Command
        {
            if (command == null)
            {
                propertyGrid.SelectedObject = null;
                return;
            }

            m_CurrentObject = new CommandProperties<Command>(command);
            ApplyDynamicTypeDescriptorToPropertyView();
        }

        private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            var command = m_CurrentObject.m_Command;
            ScriptManager.Instance.GetScriptFromCommand(command).ApplyCommandModifications(command);

            ApplyDynamicTypeDescriptorToPropertyView();
        }

        private void ApplyDynamicTypeDescriptorToPropertyView()
        {
            DynamicTypeDescriptor dt = new DynamicTypeDescriptor(m_CurrentObject.GetType());

            m_CurrentObject.HideProperties(dt);
            m_CurrentObject.OnPropertiesModified();
            propertyGrid.SelectedObject = dt.FromComponent(m_CurrentObject);
        }
    }
}
