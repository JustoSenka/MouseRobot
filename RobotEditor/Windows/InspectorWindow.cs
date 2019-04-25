using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotEditor.Inspector;
using RobotEditor.Settings;
using RobotEditor.Utils;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Execution;
using RobotRuntime.Recordings;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Unity;
using Unity.Lifetime;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor.Windows
{
    [RegisterTypeToContainer(typeof(IInspectorWindow), typeof(ContainerControlledLifetimeManager))]
    public partial class InspectorWindow : DockContent, IInspectorWindow
    {
        private BaseProperties m_CurrentObject;
        private Command m_OldCommand;

        private new IUnityContainer Container;
        private ILogger Logger;
        private ITypeCollector<CommandProperties> TypeCollector;
        public InspectorWindow(IUnityContainer Container, ILogger Logger, ITypeCollector<CommandProperties> TypeCollector)
        {
            this.Container = Container;
            this.Logger = Logger;
            this.TypeCollector = TypeCollector;

            InitializeComponent();
            propertyGrid.SelectedObject = null;
        }

        public void ShowObject(object obj, IBaseHierarchyManager BaseHierarchyManager = null)
        {
            if (obj == null)
            {
                propertyGrid.SelectedObject = null;
                return;
            }

            var type = obj.GetType();
            if (typeof(Recording).IsAssignableFrom(type))
                ShowRecording((Recording)obj);

            else if (typeof(Command).IsAssignableFrom(type))
                ShowCommand((Command)obj, BaseHierarchyManager);

            if (m_CurrentObject != null)
            {
                m_CurrentObject.BaseHierarchyManager = BaseHierarchyManager;
                ApplyDynamicTypeDescriptorToPropertyView();
            }
        }

        private void ShowRecording<T>(T recording) where T : Recording
        {
            var recordingProperties = Container.Resolve<Inspector.RecordingProperties>();
            recordingProperties.Recording = recording;

            m_CurrentObject = recordingProperties;
        }

        private void ShowCommand<T>(T command, IBaseHierarchyManager BaseHierarchyManager) where T : Command
        {
            var designerType = GetDesignerTypeForCommand(command.GetType());

            var commandProperties = (CommandProperties)Container.Resolve(designerType);

            m_OldCommand = command;

            commandProperties.Command = command;
            m_CurrentObject = commandProperties;
            m_CurrentObject.BaseHierarchyManager = BaseHierarchyManager;
        }

        private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            if (m_CurrentObject is Inspector.RecordingProperties recordingProperties)
            {
                ApplyDynamicTypeDescriptorToPropertyView();
            }
            else if (typeof(CommandProperties).IsAssignableFrom(m_CurrentObject.GetType()))
            {
                var commandProperties = (CommandProperties)m_CurrentObject;
                var command = commandProperties.Command;
                m_CurrentObject.BaseHierarchyManager.GetRecordingFromCommand(command).ApplyCommandModifications(command);

                // Command type has changed, so we might need new command properties instance for it, so inspector draws int properly
                if (m_OldCommand.GetType() != command.GetType())
                    ShowCommand(command, m_CurrentObject.BaseHierarchyManager);

                ApplyDynamicTypeDescriptorToPropertyView();
            }
        }

        private void ApplyDynamicTypeDescriptorToPropertyView()
        {
            if (m_CurrentObject == null)
                return;

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

            // Set help text. Mostly used by Unknown Commands
            if (!m_CurrentObject.HelpTextTitle.IsEmpty())
                SetHelpText(m_CurrentObject.HelpTextTitle, m_CurrentObject.HelpTextContent);
            else
                propertyGrid.HelpVisible = false;
        }

        private Type GetDesignerTypeForCommand(Type commandType)
        {
            var attribute = commandType.GetCustomAttributes(false).OfType<PropertyDesignerTypeAttribute>().FirstOrDefault();
            var type = attribute != null ? TypeCollector.AllTypes.FirstOrDefault(t => t.Name == attribute.typeName) : null;

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

        private Control docComment = null;
        private void SetHelpText(string title, string helpText)
        {
            FindAndCacheDocCommentControl();

            ResizeDescriptionArea(propertyGrid, Regex.Matches(helpText, Environment.NewLine).Count);
            propertyGrid.HelpVisible = true;

            // Might not yet be initialized if ran on startup, just ignore
            if (docComment == null)
                return;

            var aFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var aInfo = docComment.GetType().GetMethod("SetComment", aFlags);
            aInfo?.Invoke(docComment, new object[] { title, helpText });
        }

        private void ResizeDescriptionArea(PropertyGrid grid, int nNumLines)
        {
            FindAndCacheDocCommentControl();
            if (docComment == null)
                return;

            docComment.SetPropertyIfExist("Lines", nNumLines + 3);
            docComment.SetFieldIfExist("userSized", true);
        }

        private void FindAndCacheDocCommentControl()
        {
            if (docComment == null)
            {
                var controls = propertyGrid.GetPropertyIfExist("Controls") as Control.ControlCollection;
                foreach (Control control in controls)
                {
                    if (control.GetType().Name == "DocComment")
                        docComment = control;
                }
            }
        }
    }
}
