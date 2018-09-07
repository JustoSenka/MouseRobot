using RobotRuntime.Settings;
using System;

namespace Robot.Settings
{
    [Serializable]
    public class CompilerSettings : BaseSettings
    {
        public string[] CompilerReferences { get; set; }
    }
}
