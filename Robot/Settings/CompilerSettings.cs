using Newtonsoft.Json;
using RobotRuntime;
using RobotRuntime.Settings;
using System;
using System.Linq;

namespace Robot.Settings
{
    [Serializable]
    public class CompilerSettings : BaseSettings
    {
        [JsonIgnore]
        public static string[] DefaultRobotReferences => AppDomain.CurrentDomain.GetAllAssembliesInBaseDirectory().Distinct().ToArray();

        [JsonIgnore]
        [NonSerialized]
        public readonly static string[] DefaultCompilerReferences = new[]
        {
            "System.dll",
            "System.Drawing.dll",
            "System.Windows.Forms.dll",
            "System.Core.dll"
        };

        [JsonIgnore]
        public bool HasValidReferences => CompilerReferences != null && CompilerReferences.Any(r => r.Trim() != "");

        [JsonIgnore]
        public string[] NonEmptyCompilerReferences => CompilerReferences?.Where(r => r.Trim() != "").ToArray();

        public string[] CompilerReferences { get; set; } = new string[0];
    }
}
