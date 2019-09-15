using Robot.Abstractions;
using Robot.Settings;
using RobotEditor.Utils;
using RobotRuntime.Settings;
using RobotRuntime;
using System;
using System.ComponentModel;

namespace RobotEditor.Settings
{
    [Serializable]
    [PropertyDesignerType(typeof(CompilerSettings))]
    public class CompilerProperties : BaseProperties
    {
        [NonSerialized]
        private CompilerSettings m_Settings;

        [NonSerialized]
        private IScriptManager ScriptManager;

        [Browsable(false)]
        public override string Title { get { return "Compiler Settings"; } }

        public CompilerProperties(BaseSettings settings, IScriptManager ScriptManager) : base(settings)
        {
            this.m_Settings = (CompilerSettings)settings;
            this.ScriptManager = ScriptManager;
        }

        private const int NumOfCategories = 1;
        private const int CompilerSettings = 1;

        [SortedCategory("Compiler Settings", CompilerSettings, NumOfCategories)]
        [DefaultValue(new string[0])]
        [DisplayName("Referenced Assemblies")]
        public string[] CompilerReferences
        {
            get { return m_Settings.CompilerReferences; }
            set { m_Settings.CompilerReferences = value; }
        }

        public override void OnPropertiesModified()
        {
            ScriptManager.CompileScriptsAndReloadUserDomain();
        }
    }
}
