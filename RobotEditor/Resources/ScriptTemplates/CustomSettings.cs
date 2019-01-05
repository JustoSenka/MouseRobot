using RobotRuntime.Settings;
using System;
using System.Windows.Forms;

namespace RobotEditor.Resources.ScriptTemplates
{
    [Serializable]
    public class UserSettings : BaseSettings 
    { 
        public int SomeInt { get; set; } = 15;
        public string SomeString { get; set; } = "string";
        public bool SomeBool { get; set; } = false;
        public Keys SomeKey { get; set; } = Keys.E;
    }
}
