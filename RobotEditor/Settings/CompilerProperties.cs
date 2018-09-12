using Robot.Abstractions;
using Robot.Settings;
using RobotEditor.Utils;
using RobotRuntime.Settings;
using System;
using System.ComponentModel;

namespace RobotEditor.Settings
{
    [Serializable]
    public class CompilerProperties : BaseProperties
    {
        [NonSerialized]
        private CompilerSettings m_Settings;

        [NonSerialized]
        private IPluginManager PluginManager;

        [Browsable(false)]
        public override string Title { get { return "Compiler Settings"; } }

        public CompilerProperties(BaseSettings settings, IPluginManager PluginManager)
        {
            this.m_Settings = (CompilerSettings)settings;
            this.PluginManager = PluginManager;
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
            PluginManager.CompileScriptsAndReloadUserDomain();
        }
    }
}
