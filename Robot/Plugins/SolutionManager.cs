using Robot.Abstractions;
using Robot.Properties;
using Robot.Settings;
using RobotRuntime;
using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Robot.Plugins
{
    public class SolutionManager : ISolutionManager
    {
        private string m_ProjectGuid;
        private string m_SolutionGuid;

        private string CSharpSolutionName => ProjectManager.ProjectName + "_Solution";
        private string CSharpProjectName => ProjectManager.ProjectName + "_Project";

        private const string k_DotNetVersion = "v4.6.1";
        private const string k_ReferenceWrapper = "\t<Reference Include=\"{0}\"/>";
        private const string k_CompileWrapper = "\t<Compile Include=\"{0}\"/>";

        private readonly IAssetManager AssetManager;
        private readonly IProjectManager ProjectManager;
        private readonly IModifiedAssetCollector ModifiedAssetCollector;
        private readonly ISettingsManager SettingsManager;
        public SolutionManager(IAssetManager AssetManager, IProjectManager ProjectManager, IPluginLoader PluginManager,
            IModifiedAssetCollector ModifiedAssetCollector, ISettingsManager SettingsManager)
        {
            this.AssetManager = AssetManager;
            this.ProjectManager = ProjectManager;
            this.ModifiedAssetCollector = ModifiedAssetCollector;
            this.SettingsManager = SettingsManager;

            m_SolutionGuid = Guid.NewGuid().ToString();
            m_ProjectGuid = Guid.NewGuid().ToString();

            //TODO: Fix me. Generate project only when Scripts and Plugins are ADDED/REMOVED // If project is identical, it will not be overwritten
            //TODO: Add menu in assets window to regenerate everything
            //TODO: Changing compiler settings should regenerate project
            ModifiedAssetCollector.ExtensionFilters.Add(FileExtensions.PluginD);
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
            var slnPath = CSharpSolutionName + ".sln";

            var upperProjGuid = m_ProjectGuid.ToUpper();
            var upperSlnGuid = m_SolutionGuid.ToUpper();

            var newSln = string.Format(Resources.SolutionTemplate,
                CSharpProjectName, // ProjectName
                CSharpProjectName + ".csproj",  // Project Path
                upperProjGuid, // Project GUID
                upperSlnGuid); // Solution GUID

            var currentSln = (File.Exists(slnPath)) ? File.ReadAllText(slnPath) : "";
            if (newSln.Trim().Equals(currentSln.Trim(), StringComparison.InvariantCultureIgnoreCase))
                return;

            File.WriteAllText(slnPath, newSln);
        }

        public void GenerateNewProject()
        {
            var projPath = CSharpProjectName + ".csproj";

            var refs = CollectAllCompilerReferences().Distinct().Select((s) => string.Format(k_ReferenceWrapper, s));
            var refString = string.Join("\n", refs).Trim('\n');

            var sources = AssetManager.Assets.Where(a => a.Path.EndsWith(FileExtensions.PluginD)).Distinct().Select((s) => string.Format(k_CompileWrapper, s.Path));
            var sourcesString = string.Join("\n", sources).Trim('\n');

            var newProj = string.Format(Resources.ProjectTemplate,
                m_ProjectGuid, // Project GUID
                ProjectManager.ProjectName, // RootNamespace
                CSharpProjectName, // AssemblyName
                k_DotNetVersion,  // Project .Net Version
                refString, // References
                sourcesString); // Source files

            var currentProj = File.Exists(projPath) ? File.ReadAllText(projPath) : "";
            if (newProj.Trim().Equals(currentProj.Trim(), StringComparison.InvariantCultureIgnoreCase))
                return;

            File.WriteAllText(projPath, newProj);
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
        }
    }
}
