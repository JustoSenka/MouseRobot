using RobotEditor.Scripts;
using RobotEditor.Abstractions;
using RobotEditor.Utils;
using RobotRuntime;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Robot.Abstractions;
using RobotRuntime.Abstractions;

namespace RobotEditor.Windows
{
    public partial class InspectorWindow : DockContent, IInspectorWindow
    {
        private CommandProperties<Command> m_CurrentObject;

        private IScriptManager ScriptManager;
        private IAssetManager AssetManager;
        private IAssetGuidManager AssetGuidManager;
        public InspectorWindow(IScriptManager ScriptManager, IAssetManager AssetManager, IAssetGuidManager AssetGuidManager)
        {
            this.ScriptManager = ScriptManager;
            this.AssetManager = AssetManager;
            this.AssetGuidManager = AssetGuidManager;

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

            m_CurrentObject = new CommandProperties<Command>(command, AssetManager, ScriptManager, AssetGuidManager);
            ApplyDynamicTypeDescriptorToPropertyView();
        }

        private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            var command = m_CurrentObject.m_Command;
            ScriptManager.GetScriptFromCommand(command).ApplyCommandModifications(command);

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
