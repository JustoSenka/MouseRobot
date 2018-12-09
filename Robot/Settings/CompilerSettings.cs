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

        public bool HasValidReferences => CompilerReferences != null && CompilerReferences.Any(r => r.Trim() != "");
        public string[] NonEmptyCompilerReferences => CompilerReferences?.Where(r => r.Trim() != "").ToArray();

        public string[] CompilerReferences { get; set; } = new string[0];
    }
}
