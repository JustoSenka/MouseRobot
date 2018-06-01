using RobotEditor.Scripts;
using RobotEditor.Abstractions;
using RobotEditor.Utils;
using RobotRuntime;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Robot.Abstractions;
using RobotRuntime.Abstractions;
using System;
using Unity;
using System.Linq;
using RobotRuntime.Execution;

namespace RobotEditor.Windows
{
    public partial class InspectorWindow : DockContent, IInspectorWindow
    {
        private CommandProperties m_CurrentObject;

        private Type[] m_NativeDesignerTypes;
        private Type[] m_UserDesignerTypes;
        private Type[] m_DesignerTypes;

        private new IUnityContainer Container;
        private IScriptManager ScriptManager;
        private IPluginLoader PluginLoader;
        private ILogger Logger;
        public InspectorWindow(IUnityContainer Container, IScriptManager ScriptManager, IPluginLoader PluginLoader, ILogger Logger)
        {
            this.ScriptManager = ScriptManager;
            this.Container = Container;
            this.PluginLoader = PluginLoader;
            this.Logger = Logger;

            InitializeComponent();
            propertyGrid.SelectedObject = null;

            PluginLoader.UserDomainReloaded += OnDomainReloaded;

            CollectNativeCommands();
            CollectUserCommands();
        }

        private void OnDomainReloaded()
        {
            CollectUserCommands();
        }

        private void CollectNativeCommands()
        {
            m_NativeDesignerTypes = AppDomain.CurrentDomain.GetNativeAssemblies().GetAllTypesWhichImplementInterface(typeof(CommandProperties)).ToArray();
        }

        private void CollectUserCommands()
        {
            // DO-DOMAIN: This will not work if assemblies are in different domain
            m_UserDesignerTypes = PluginLoader.IterateUserAssemblies(a => a).GetAllTypesWhichImplementInterface(typeof(CommandProperties)).ToArray();
            m_DesignerTypes = m_NativeDesignerTypes.Concat(m_UserDesignerTypes).ToArray();
        }

        public void ShowCommand<T>(T command) where T : Command
        {
            if (command == null)
            {
                propertyGrid.SelectedObject = null;
                return;
            }

            var designerType = GetDesignerTypeForCommand(command.GetType());
            m_CurrentObject = (CommandProperties)Container.Resolve(designerType);

            m_CurrentObject.Command = command;
            ApplyDynamicTypeDescriptorToPropertyView();
        }

        private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            var command = m_CurrentObject.Command;
            ScriptManager.GetScriptFromCommand(command).ApplyCommandModifications(command);

            ApplyDynamicTypeDescriptorToPropertyView();
        }

        private void ApplyDynamicTypeDescriptorToPropertyView()
        {
            var designerType = m_CurrentObject.GetType();
            var isDesignerDefault = designerType == typeof(DefaultCommandProperties);

            DynamicTypeDescriptor dt = isDesignerDefault ? new DynamicTypeDescriptor(m_CurrentObject.Command.GetType()) : new DynamicTypeDescriptor(m_CurrentObject.GetType());

            m_CurrentObject.HideProperties(ref dt);
            m_CurrentObject.OnPropertiesModified();

            propertyGrid.SelectedObject = isDesignerDefault ? dt.FromComponent(m_CurrentObject.Command) : dt.FromComponent(m_CurrentObject);
        }

        private Type GetDesignerTypeForCommand(Type commandType)
        {
            var attribute = commandType.GetCustomAttributes(false).OfType<PropertyDesignerTypeAttribute>().FirstOrDefault();
            var type = attribute != null ? m_DesignerTypes.FirstOrDefault(t => t.Name == attribute.typeName) : null;

            if (attribute == null) // No attribute, return default without error
            {
                return typeof(DefaultCommandProperties);
            }
            else if (type == null) // Had attribute but type was not found, log error
            {
                Logger.Logi(LogType.Error, "Property Designer for command type '" + commandType.FullName + "' not found. Returning default property designer.");
                return typeof(DefaultCommandProperties);
            }

            return type;
        }
    }
}
