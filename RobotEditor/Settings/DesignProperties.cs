using Robot.Settings;
using RobotEditor.Utils;
using RobotRuntime.Settings;
using RobotRuntime;
using System;
using System.ComponentModel;

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
        private const int Hierarchy = 1;

        [SortedCategory("Hierarchy font", Hierarchy, NumOfCategories)]
        [DefaultValue("Consolas")]
        [DisplayName("Window state when running tests")]
        public string Font
        {
            get { return m_Settings.Font; }
            set { m_Settings.Font = value; }
        }
    }
}
