using Robot.Abstractions;
using Robot.Settings;
using RobotEditor.Abstractions;
using RobotEditor.CustomControls;
using RobotEditor.Settings;
using RobotEditor.Utils;
using RobotRuntime;
using RobotRuntime.Scripts;
using RobotRuntime.Settings;
using System;
using System.Linq;
using System.Windows.Forms;
using Unity.Lifetime;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor
{
    [RegisterTypeToContainer(typeof(IPropertiesWindow), typeof(ContainerControlledLifetimeManager))]
    public partial class PropertiesWindow : DockContent, IPropertiesWindow
    {
        public event Action<BaseProperties> PropertiesModified;

        private BaseSettings m_CurrentSettings;
        private BaseProperties m_CurrentObject;
        private Type m_CurrentObjectType;
        private Type m_CurrentSettingsType;

        private readonly ISettingsManager SettingsManager;
        private readonly IScriptManager ScriptManager;
        private readonly TypeCollector<BaseProperties> TypeCollector;
        public PropertiesWindow(ISettingsManager SettingsManager, IScriptManager ScriptManager, TypeCollector<BaseProperties> TypeCollector)
        {
            this.SettingsManager = SettingsManager;
            this.ScriptManager = ScriptManager;
            this.TypeCollector = TypeCollector;

            SettingsManager.SettingsRestored += OnSettingsRestored;

            InitializeComponent();
            propertyGrid.HandleCreated += (sender, args) =>
                ShowSettings(SettingsManager.GetSettings<RecordingSettings>());
        }

        public void UpdateCurrentProperties()
        {
            // Propertoes window is not open
            if (m_CurrentObject == null || m_CurrentSettingsType == null)
                return;

            ShowSettings(SettingsManager.GetSettingsFromType(m_CurrentSettingsType));
        }

        private void OnSettingsRestored()
        {
            CreateContextMenuItemsForSettings();

            if (m_CurrentObject == null || m_CurrentSettingsType == null)
                return;

            var newSettings = SettingsManager.GetSettingsFromType(m_CurrentSettingsType);
            ShowSettings(newSettings);
        }

        private void CreateContextMenuItemsForSettings()
        {
            contextMenuStrip.Items.Clear();
            var newContextMenuItems = SettingsManager.Settings.Select(s =>
            {
                var item = new TrackedToolStripMenuItem(s.FriendlyName);
                item.Click += (_, __) => ShowSettings(SettingsManager.GetSettingsFromType(s.GetType()));
                return item;
            }).ToList();

            newContextMenuItems.Sort(new ToolStripMenuItemComparer());
            contextMenuStrip.Items.AddRange(newContextMenuItems.ToArray());
        }

        public void ShowProperties<T>(T properties) where T : BaseProperties
        {
            /* not used. Might be useful in future
             m_CurrentObject = properties;
             m_CurrentObjectType = properties.GetType();
             this.Text = properties.Title;
             propertyGrid_PropertyValueChanged(this, null);*/
        }

        public void ShowSettings<T>(T settings) where T : BaseSettings
        {
            m_CurrentSettings = settings;
            m_CurrentObject = WrapSettingsToProperties(settings);

            this.Text = m_CurrentObject == null ? settings.FriendlyName : m_CurrentObject.Title;

            // Putting null as sender so we don't call properties modified callback. But we want to update dt
            propertyGrid_PropertyValueChanged(null, null);
        }

        private BaseProperties WrapSettingsToProperties<T>(T settings) where T : BaseSettings
        {
            m_CurrentSettingsType = settings.GetType();

            if (settings is CompilerSettings)
            {
                m_CurrentObjectType = typeof(CompilerProperties);
                return new CompilerProperties(settings, ScriptManager);
            }
            else
            {
                m_CurrentObjectType = TypeCollector.AllTypes
                    .FirstOrDefault(t => t.GetCustomAttributes(false).OfType<PropertyDesignerTypeAttribute>()?
                    .FirstOrDefault()?.typeName == m_CurrentSettingsType.Name);

                if (m_CurrentObjectType != null)
                    return (BaseProperties)Activator.CreateInstance(m_CurrentObjectType, settings);
            }

            return null;
        }

        private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            if (m_CurrentObject != null)
            {
                DynamicTypeDescriptor dt = new DynamicTypeDescriptor(m_CurrentObjectType);

                m_CurrentObject.HideProperties(ref dt);

                propertyGrid.InvokeIfCreated(new MethodInvoker(() =>
                {
                    propertyGrid.SelectedObject = dt.FromComponent(m_CurrentObject);
                }));

                if (sender != null) // If it was modified from UI, call the callback. In other cases we don't want to do that.
                {
                    m_CurrentObject.OnPropertiesModified();
                    PropertiesModified?.Invoke(m_CurrentObject);
                    SettingsManager.InvokeSettingsModifiedCallback(m_CurrentSettings);
                }
            }
            else if (m_CurrentSettings != null)
            {
                propertyGrid.InvokeIfCreated(new MethodInvoker(() =>
                {
                    propertyGrid.SelectedObject = m_CurrentSettings;
                }));

                if (sender != null)
                {
                    PropertiesModified?.Invoke(null);
                    SettingsManager.InvokeSettingsModifiedCallback(m_CurrentSettings);
                }
            }
        }
    }
}
