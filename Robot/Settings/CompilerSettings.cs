using RobotRuntime;
using RobotRuntime.Settings;
using System;
using System.Linq;

namespace Robot.Settings
{
    [Serializable]
    public class CompilerSettings : BaseSettings
    {
        public static string[] DefaultRobotReferences => AppDomain.CurrentDomain.GetAllAssembliesInBaseDirectory().Distinct().ToArray();

        [NonSerialized]
        public readonly static string[] DefaultCompilerReferences = new[]
        {
            "System.dll",
            "System.Drawing.dll",
            "System.Windows.Forms.dll",
            "System.Core.dll"
        };

        public string[] CompilerReferences { get; set; }
    }
}
