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

            //TODO: Fix me. Generate project only when Scripts and Plugins are ADDED/REMOVED
            //TODO: Fix me. Generate solution only once in a lifetime
            //TODO: Add menu in assets window to regenerate everything
            ModifiedAssetCollector.ExtensionFilters.Add(FileExtensions.PluginD);
            ModifiedAssetCollector.ExtensionFilters.Add(FileExtensions.DllD);
            ModifiedAssetCollector.AssetsModified += OnAssetsModified;
        }

        private void OnAssetsModified(IList<string> modifiedAssets)
        {
            GenerateNewProject();
            GenerateNewSolution();
        }

        public void GenerateNewSolution()
        {
            if (m_ProjectGuid == Guid.Empty.ToString())
            {
                GenerateNewProject();
                Logger.Log(LogType.Warning, "Trying to generate solution without first generating project file. " +
                    "It should be other way around, this might not be supported in future releases.");
            }

            m_SolutionGuid = Guid.NewGuid().ToString();

            var sln = string.Format(Resources.SolutionTemplate,
                CSharpProjectName, // ProjectName
                CSharpProjectName + ".csproj",  // Project Path
                m_ProjectGuid, // Project GUID
                m_SolutionGuid); // Solution GUID

            File.WriteAllText(CSharpSolutionName + ".sln", sln);
        }

        public void GenerateNewProject()
        {
            m_ProjectGuid = Guid.NewGuid().ToString();

            var refs = CollectAllCompilerReferences().Distinct().Select((s) => string.Format(k_ReferenceWrapper, s));
            var refString = string.Join("\n", refs).Trim('\n');

            var sources = AssetManager.Assets.Where(a => a.Path.EndsWith(FileExtensions.PluginD)).Distinct().Select((s) => string.Format(k_CompileWrapper, s.Path));
            var sourcesString = string.Join("\n", sources).Trim('\n');

            var proj = string.Format(Resources.ProjectTemplate,
                m_ProjectGuid, // Project GUID
                ProjectManager.ProjectName, // RootNamespace
                CSharpProjectName, // AssemblyName
                k_DotNetVersion,  // Project .Net Version
                refString, // References
                sourcesString); // Source files

            File.WriteAllText(CSharpProjectName + ".csproj", proj);
        }

        private IEnumerable<string> CollectAllCompilerReferences()
        {
            foreach (var r in CompilerSettings.DefaultRobotReferences)
                yield return r;

            foreach (var r in CompilerSettings.DefaultCompilerReferences)
                yield return r;

            var settings = SettingsManager.GetSettings<CompilerSettings>();
            if (settings != null && settings.CompilerReferences != null)
                foreach (var r in settings.CompilerReferences)
                    yield return r;
        }
    }
}
