using RobotEditor.Inspector;
using RobotEditor.Abstractions;
using RobotEditor.Utils;
using RobotRuntime;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using RobotRuntime.Abstractions;
using System;
using Unity;
using System.Linq;
using RobotRuntime.Execution;
using Robot.Scripts;
using RobotEditor.Settings;
using RobotRuntime.Scripts;

namespace RobotEditor.Windows
{
    public partial class InspectorWindow : DockContent, IInspectorWindow
    {
        private BaseProperties m_CurrentObject;
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

        public void ShowObject(object obj, BaseScriptManager BaseScriptManager = null)
        {
            if (obj == null)
            {
                propertyGrid.SelectedObject = null;
                return;
            }

            var type = obj.GetType();
            if (typeof(Script).IsAssignableFrom(type))
                ShowScript((Script)obj);

            else if (typeof(Command).IsAssignableFrom(type))
                ShowCommand((Command)obj, BaseScriptManager);

            this.BaseScriptManager = BaseScriptManager;
            m_CurrentObject.BaseScriptManager = BaseScriptManager;

            ApplyDynamicTypeDescriptorToPropertyView();
        }

        private void ShowScript<T>(T script) where T : Script
        {
            var scriptProperties = Container.Resolve<ScriptProperties>();
            scriptProperties.Script = script;

            m_CurrentObject = scriptProperties;
        }

        private void ShowCommand<T>(T command, BaseScriptManager BaseScriptManager) where T : Command
        {
            var designerType = GetDesignerTypeForCommand(command.GetType());

            var commandProperties = (CommandProperties)Container.Resolve(designerType);

            m_OldCommand = command;

            commandProperties.Command = command;
            m_CurrentObject = commandProperties;
            m_CurrentObject.BaseScriptManager = BaseScriptManager;
        }

        private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            if (m_CurrentObject is ScriptProperties scriptProperties)
            {
                ApplyDynamicTypeDescriptorToPropertyView();
            }
            else if (typeof(CommandProperties).IsAssignableFrom(m_CurrentObject.GetType()))
            {
                var commandProperties = (CommandProperties)m_CurrentObject;
                var command = commandProperties.Command;
                BaseScriptManager.GetScriptFromCommand(command).ApplyCommandModifications(command);

                // Command type has changed, so we might need new command properties instance for it, so inspector draws int properly
                if (m_OldCommand.GetType() != command.GetType())
                    ShowCommand(command, BaseScriptManager);

                ApplyDynamicTypeDescriptorToPropertyView();
            }
        }

        private void ApplyDynamicTypeDescriptorToPropertyView()
        {
            var designerType = m_CurrentObject.GetType();
            var isDesignerDefault = designerType == typeof(DefaultCommandProperties);

            var command = isDesignerDefault ? ((CommandProperties)m_CurrentObject).Command : null;

            DynamicTypeDescriptor dt = isDesignerDefault ?
                new DynamicTypeDescriptor(command.GetType()) :
                new DynamicTypeDescriptor(m_CurrentObject.GetType());

            m_CurrentObject.HideProperties(ref dt);
            m_CurrentObject.OnPropertiesModified();

            propertyGrid.SelectedObject = isDesignerDefault ?
                dt.FromComponent(command) :
                dt.FromComponent(m_CurrentObject);
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
