using RobotEditor.Inspector;
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
using Robot.Scripts;

namespace RobotEditor.Windows
{
    public partial class InspectorWindow : DockContent, IInspectorWindow
    {
        private CommandProperties m_CurrentObject;
        private Command m_OldCommand;

        private Type[] m_NativeDesignerTypes;
        private Type[] m_UserDesignerTypes;
        private Type[] m_DesignerTypes;

        private BaseScriptManager BaseScriptManager;

        private new IUnityContainer Container;
        private IPluginLoader PluginLoader;
        private ILogger Logger;
        public InspectorWindow(IUnityContainer Container, IPluginLoader PluginLoader, ILogger Logger)
        {
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

            // This will break if command is custom command, because script manager replaces all old instances with newly compiled ones, so pointer type is no good here
            /*if (m_CurrentObject != null && m_CurrentObject.Command != null)
                Invoke(new MethodInvoker(() => ShowCommand(m_CurrentObject.Command)));*/
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

        public void ShowCommand<T>(T command, BaseScriptManager BaseScriptManager) where T : Command
        {
            if (command == null)
            {
                propertyGrid.SelectedObject = null;
                return;
            }

            var designerType = GetDesignerTypeForCommand(command.GetType());
            m_CurrentObject = (CommandProperties)Container.Resolve(designerType);

            this.BaseScriptManager = BaseScriptManager;
            m_CurrentObject.BaseScriptManager = BaseScriptManager;

            m_OldCommand = command;
            m_CurrentObject.Command = command;
            ApplyDynamicTypeDescriptorToPropertyView();
        }

        private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            var command = m_CurrentObject.Command;
            BaseScriptManager.GetScriptFromCommand(command).ApplyCommandModifications(command);

            // Command type has changed, so we might need new command properties instance for it, so inspector draws int properly
            if (m_OldCommand.GetType() != command.GetType())
                ShowCommand(command, BaseScriptManager);
            // If not, updating type descriptor is enough
            else
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
