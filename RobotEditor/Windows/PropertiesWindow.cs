using RobotEditor.Abstractions;
using Robot.Settings;
using RobotEditor.Settings;
using RobotEditor.Utils;
using RobotRuntime.Settings;
using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Robot.Abstractions;

namespace RobotEditor
{
    public partial class PropertiesWindow : DockContent, IPropertiesWindow
    {
        private BaseProperties m_CurrentObject;
        private Type m_CurrentObjectType;

        private ISettingsManager SettingsManager;
        public PropertiesWindow(ISettingsManager SettingsManager)
        {
            this.SettingsManager = SettingsManager;
            InitializeComponent();
            ShowSettings(SettingsManager.GetSettings<RecordingSettings>());
        }

        public void ShowProperties<T>(T properties) where T : BaseProperties
        {
            m_CurrentObject = properties;
            m_CurrentObjectType = properties.GetType();
            this.Text = properties.Title;
            propertyGrid_PropertyValueChanged(this, null);
        }

        public void ShowSettings<T>(T settings) where T : BaseSettings
        {
            m_CurrentObject = WrapSettingsToProperties(settings, ref m_CurrentObjectType);
            this.Text = m_CurrentObject.Title;
            propertyGrid_PropertyValueChanged(this, null);
        }

        private static BaseProperties WrapSettingsToProperties<T>(T settings, ref Type type) where T : BaseSettings
        {
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

            throw new ArgumentException(typeof(T) + " is not known type of settings, or property wrapper was not created for it");
        }

        private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            DynamicTypeDescriptor dt = new DynamicTypeDescriptor(m_CurrentObjectType);

            m_CurrentObject.HideProperties(dt);
            m_CurrentObject.OnPropertiesModified();
            propertyGrid.SelectedObject = dt.FromComponent(m_CurrentObject);
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

        #endregion
    }
}
