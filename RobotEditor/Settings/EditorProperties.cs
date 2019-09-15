using Robot.Settings;
using RobotEditor.Utils;
using RobotRuntime.Settings;
using RobotRuntime;
using System;
using System.ComponentModel;

namespace RobotEditor.Settings
{
    [Serializable]
    [PropertyDesignerType(typeof(EditorSettings))]
    public class EditorProperties : BaseProperties
    {
        [NonSerialized]
        private EditorSettings m_Settings;

        [Browsable(false)]
        public override string Title { get { return "Editor Settings"; } }

        public EditorProperties(BaseSettings settings) : base(settings)
        {
            this.m_Settings = (EditorSettings)settings;
        }

        private const int NumOfCategories = 1;
        private const int EditorSettings = 1;

        [SortedCategory("Editor Settings", EditorSettings, NumOfCategories)]
        [DefaultValue(WindowState.DoNothing)]
        [DisplayName("Window state when running tests")]
        public WindowState PlayingAction
        {
            get { return m_Settings.PlayingAction; }
            set { m_Settings.PlayingAction = value; }
        }

        [SortedCategory("Editor Settings", EditorSettings, NumOfCategories)]
        [DefaultValue(WindowState.DoNothing)]
        [DisplayName("Window state when running recording")]
        public WindowState RecordingAction
        {
            get { return m_Settings.RecordingAction; }
            set { m_Settings.RecordingAction = value; }
        }
    }
}
