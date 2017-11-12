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
        private BaseProperties m_CurrentObject;
        private Type m_CurrentObjectType;
        private Command m_Command;

        public InspectorWindow()
        {
            InitializeComponent();
            //ShowSettings(SettingsManager.Instance.RecordingSettings);
        }

        public void ShowCommand<T>(T command) where T : Command
        {
            m_Command = command;
            m_CurrentObject = WrapCommandsToProperties(command, ref m_CurrentObjectType);
            propertyGrid_PropertyValueChanged(this, null);
        }

        private static BaseProperties WrapCommandsToProperties<T>(T command, ref Type type) where T : Command
        {
            if (command is CommandMove)
            {
                type = typeof(CommandMoveProperties);
                return new CommandMoveProperties(command as CommandMove);
            }

            throw new ArgumentException(typeof(T) + " is not known type of settings, or property wrapper was not created for it");
        }

        private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            DynamicTypeDescriptor dt = new DynamicTypeDescriptor(m_CurrentObjectType);

            ScriptManager.Instance.GetScriptFromCommand(m_Command).ApplyCommandModifications(m_Command);

            m_CurrentObject.HideProperties(dt);
            m_CurrentObject.OnPropertiesModified();
            propertyGrid.SelectedObject = dt.FromComponent(m_CurrentObject);
        }
    }
}
