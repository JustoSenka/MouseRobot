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

        private const int NumOfCategories = 2;
        private const int ThemeSettings = 1;
        private const int MiscSettings = 2;

        [SortedCategory("Theme Settings", ThemeSettings, NumOfCategories)]
        [DefaultValue(Theme.Dark)]
        [DisplayName("Window Theme")]
        public Theme Theme
        {
            get { return m_Settings.Theme; }
            set { m_Settings.Theme = value; }
        }

        [SortedCategory("Misc Settings", MiscSettings, NumOfCategories)]
        [DefaultValue(WindowState.DoNothing)]
        [DisplayName("Window state when running tests")]
        public WindowState PlayingAction
        {
            get { return m_Settings.PlayingAction; }
            set { m_Settings.PlayingAction = value; }
        }

        [SortedCategory("Misc Settings", MiscSettings, NumOfCategories)]
        [DefaultValue(WindowState.DoNothing)]
        [DisplayName("Window state when running recording")]
        public WindowState RecordingAction
        {
            get { return m_Settings.RecordingAction; }
            set { m_Settings.RecordingAction = value; }
        }


    }
}
