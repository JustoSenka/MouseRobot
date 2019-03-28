using Robot.Abstractions;
using Robot.Properties;
using Robot.Settings;
using RobotRuntime;
using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Robot.Scripts
{
    public class SolutionManager : ISolutionManager
    {
        public string CSharpSolutionPath => CSharpSolutionName + ".sln";
        public string CSharpProjectPath => CSharpProjectName + ".csproj";

        public string CSharpSolutionName => ProjectManager.ProjectName + "_Solution";
        public string CSharpProjectName => ProjectManager.ProjectName + "_Project";

        private string m_ProjectGuid;
        private string m_SolutionGuid;

        private const string k_DotNetVersion = "v4.6.1";
        private const string k_ReferenceWrapper = "\t<Reference Include=\"{0}\"/>";
        private const string k_CompileWrapper = "\t<Compile Include=\"{0}\"/>";

        private readonly IAssetManager AssetManager;
        private readonly IProjectManager ProjectManager;
        private readonly IModifiedAssetCollector ModifiedAssetCollector;
        private readonly ISettingsManager SettingsManager;
        public SolutionManager(IAssetManager AssetManager, IProjectManager ProjectManager, 
            IModifiedAssetCollector ModifiedAssetCollector, ISettingsManager SettingsManager)
        {
            this.AssetManager = AssetManager;
            this.ProjectManager = ProjectManager;
            this.ModifiedAssetCollector = ModifiedAssetCollector;
            this.SettingsManager = SettingsManager;

            m_SolutionGuid = Guid.NewGuid().ToString();
            m_ProjectGuid = Guid.NewGuid().ToString();

            //TODO: Fix me. Generate project only when Recordings and Scripts are ADDED/REMOVED // If project is identical, it will not be overwritten
            //TODO: Add menu in assets window to regenerate everything
            //TODO: Changing compiler settings should regenerate project
            ModifiedAssetCollector.ExtensionFilters.Add(FileExtensions.ScriptD);
            ModifiedAssetCollector.ExtensionFilters.Add(FileExtensions.DllD);
            ModifiedAssetCollector.AssetsModified += (assets) => RegenerateEverything();

            ProjectManager.NewProjectOpened += (path) => RegenerateEverything();
            SettingsManager.SettingsRestored += RegenerateEverything;
        }

        private void RegenerateEverything()
        {
            GenerateNewProject();
            GenerateNewSolution();
        }

        public void GenerateNewSolution()
        {
            var upperProjGuid = m_ProjectGuid.ToUpper();
            var upperSlnGuid = m_SolutionGuid.ToUpper();

            var newSln = string.Format(Resources.SolutionTemplate,
                CSharpProjectName, // ProjectName
                CSharpProjectName + ".csproj",  // Project Path
                upperProjGuid, // Project GUID
                upperSlnGuid); // Solution GUID

            var currentSln = (File.Exists(CSharpSolutionPath)) ? File.ReadAllText(CSharpSolutionPath) : "";
            if (newSln.Trim().Equals(currentSln.Trim(), StringComparison.InvariantCultureIgnoreCase))
                return;

            File.WriteAllText(CSharpSolutionPath, newSln);
        }

        public void GenerateNewProject()
        {
            var refs = CollectAllCompilerReferences().Distinct().Select((s) => string.Format(k_ReferenceWrapper, s));
            var refString = string.Join("\n", refs).Trim('\n');

            var sources = AssetManager.Assets.Where(a => a.Path.EndsWith(FileExtensions.ScriptD)).Distinct().Select((s) => string.Format(k_CompileWrapper, s.Path));
            var sourcesString = string.Join("\n", sources).Trim('\n');

            var newProj = string.Format(Resources.ProjectTemplate,
                m_ProjectGuid, // Project GUID
                ProjectManager.ProjectName, // RootNamespace
                CSharpProjectName, // AssemblyName
                k_DotNetVersion,  // Project .Net Version
                refString, // References
                sourcesString); // Source files

            var currentProj = File.Exists(CSharpProjectPath) ? File.ReadAllText(CSharpProjectPath) : "";
            if (newProj.Trim().Equals(currentProj.Trim(), StringComparison.InvariantCultureIgnoreCase))
                return;

            File.WriteAllText(CSharpProjectPath, newProj);
        }

        private IEnumerable<string> CollectAllCompilerReferences()
        {
            foreach (var r in CompilerSettings.DefaultRobotReferences)
                yield return r;

            foreach (var r in CompilerSettings.DefaultCompilerReferences)
                yield return r;

            var settings = SettingsManager.GetSettings<CompilerSettings>();
            if (settings != null)
            {
                if (settings.HasValidReferences)
                    foreach (var r in settings.NonEmptyCompilerReferences)
                        yield return r;
            }

            var allPluginAssets = AssetManager.Assets.Where(a => a.Importer.HoldsType() == typeof(Assembly));
            foreach (var a in allPluginAssets)
                yield return a.Importer.Path;
        }
    }
}
