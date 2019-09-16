using Robot.Settings;
using RobotEditor.Utils;
using RobotRuntime.Settings;
using RobotRuntime;
using System;
using System.ComponentModel;
using System.Drawing;

namespace RobotEditor.Settings
{
    [Serializable]
    [PropertyDesignerType(typeof(DesignSettings))]
    public class DesignProperties : BaseProperties
    {
        [NonSerialized]
        private DesignSettings m_Settings;

        [Browsable(false)]
        public override string Title { get { return "Editor Settings"; } }

        public DesignProperties(BaseSettings settings) : base(settings)
        {
            this.m_Settings = (DesignSettings)settings;
        }

        private const int NumOfCategories = 1;
        private const int Fonts = 1;

        private const string FONT_MS_SANS_SERIF = "Microsoft Sans Serif, 8.25pt, style=Regular";
        private const string FONT_CONSOLAS = "Consolas, 11pt, style=Regular";

        [SortedCategory("Fonts", Fonts, NumOfCategories)]
        [DefaultValue(typeof(Font), FONT_MS_SANS_SERIF)]
        [DisplayName("Default Window Font")]
        public Font DefaultWindowFont
        {
            get { return m_Settings.DefaultWindowFont; }
            set { m_Settings.DefaultWindowFont = value; }
        }

        [SortedCategory("Fonts", Fonts, NumOfCategories)]
        [DefaultValue(typeof(Font), FONT_CONSOLAS)]
        [DisplayName("Hierarchy Based Windows Font")]
        public Font HierarchyWindowsFont
        {
            get { return m_Settings.HierarchyWindowsFont; }
            set { m_Settings.HierarchyWindowsFont = value; }
        }

        [SortedCategory("Fonts", Fonts, NumOfCategories)]
        [DefaultValue(typeof(Font), FONT_MS_SANS_SERIF)]
        [DisplayName("Assets Window Font")]
        public Font AssetsWindowFont
        {
            get { return m_Settings.AssetsWindowFont; }
            set { m_Settings.AssetsWindowFont = value; }
        }

        [SortedCategory("Fonts", Fonts, NumOfCategories)]
        [DefaultValue(typeof(Font), FONT_MS_SANS_SERIF)]
        [DisplayName("Test Runner Window Font")]
        public Font TestRunnerWindowFont
        {
            get { return m_Settings.TestRunnerWindowFont; }
            set { m_Settings.TestRunnerWindowFont = value; }
        }
    }
}
