using Robot.Abstractions;
using Robot.Settings;
using RobotEditor.Abstractions;
using RobotEditor.Settings;
using RobotEditor.Utils;
using RobotRuntime.Settings;
using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor
{
    public partial class PropertiesWindow : DockContent, IPropertiesWindow
    {
        private BaseProperties m_CurrentObject;
        private Type m_CurrentObjectType;
        private Type m_CurrentSettingsType;

        private ISettingsManager SettingsManager;
        private IPluginManager PluginManager;
        public PropertiesWindow(ISettingsManager SettingsManager, IPluginManager PluginManager)
        {
            this.SettingsManager = SettingsManager;
            this.PluginManager = PluginManager;

            SettingsManager.SettingsRestored += OnSettingsRestored;

            InitializeComponent();
            ShowSettings(SettingsManager.GetSettings<RecordingSettings>());
        }

        private void OnSettingsRestored()
        {
            if (m_CurrentObject == null || m_CurrentSettingsType == null)
                return;

            var newSettings = SettingsManager.GetSettingsFromType(m_CurrentSettingsType);
            ShowSettings(newSettings);
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
            m_CurrentObject = WrapSettingsToProperties(settings, ref m_CurrentObjectType);
            this.Text = m_CurrentObject.Title;

            // Putting null as sender so we don't call properties modified callback. But we want to update dt
            propertyGrid_PropertyValueChanged(null, null);
        }

        private BaseProperties WrapSettingsToProperties<T>(T settings, ref Type type) where T : BaseSettings
        {
            m_CurrentSettingsType = settings.GetType();

            if (settings is RecordingSettings)
            {
                type = typeof(RecordingProperties);
                return new RecordingProperties(settings);
            }

            if (settings is FeatureDetectionSettings)
            {
                type = typeof(FeatureDetectionProperties);
                return new FeatureDetectionProperties(settings);
            }

            if (settings is CompilerSettings)
            {
                type = typeof(CompilerProperties);
                return new CompilerProperties(settings, PluginManager);
            }

            throw new ArgumentException(typeof(T) + " is not known type of settings, or property wrapper was not created for it");
        }

        private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            DynamicTypeDescriptor dt = new DynamicTypeDescriptor(m_CurrentObjectType);

            m_CurrentObject.HideProperties(ref dt);

            if (sender != null) // If it was modified from UI, call the callback. In other cases we don't want to do that.
                m_CurrentObject.OnPropertiesModified();

            propertyGrid.BeginInvokeIfCreated(new MethodInvoker(() =>
            {
                propertyGrid.SelectedObject = dt.FromComponent(m_CurrentObject);
            }));
        }

        #region Context Menu Items

        private void recordingSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSettings(SettingsManager.GetSettings<RecordingSettings>());
        }

        private void imageDetectionSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSettings(SettingsManager.GetSettings<FeatureDetectionSettings>());
        }

        private void compilerSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSettings(SettingsManager.GetSettings<CompilerSettings>());
        }

        #endregion
    }
}
