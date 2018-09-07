using Robot.Settings;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using RobotEditor.Utils;
using RobotRuntime.Settings;

namespace RobotEditor.Settings
{
    [Serializable]
    public class CompilerProperties : BaseProperties
    {
        [NonSerialized]
        private CompilerSettings m_Settings;

        [Browsable(false)]
        public override string Title { get { return "Compiler Settings"; } }

        public CompilerProperties(BaseSettings settings)
        {
            this.m_Settings = (CompilerSettings)settings;
        }

        private const int NumOfCategories = 1;
        private const int CompilerSettings = 1;

        [SortedCategory("Compiler Settings", CompilerSettings, NumOfCategories)]
        [DisplayName("Referenced Assemblies")]
        public string[] CompilerReferences
        {
            get { return m_Settings.CompilerReferences; }
            set { m_Settings.CompilerReferences = value; }
        }
    }
}
