using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Robot.Plugins
{
    public class PluginManager : IPluginManager
    {
        private IList<string> m_ModifiedFilesSinceLastRecompilation = new List<string>();

        private string CustomAssemblyName { get { return "CustomAssembly.dll"; } }
        private string CustomAssemblyPath { get { return Path.Combine(Paths.MetadataPath, CustomAssemblyName); } }

        private PluginCompiler PluginCompiler;
        private IPluginLoader PluginLoader;
        private IAssetManager AssetManager;
        private IProfiler Profiler;
        public PluginManager(IPluginLoader PluginLoader, IAssetManager AssetManager, IProfiler Profiler)
        {
            this.PluginLoader = PluginLoader;
            this.AssetManager = AssetManager;
            this.Profiler = Profiler;

            AssetManager.AssetCreated += AddPathToList;
            AssetManager.AssetUpdated += AddPathToList;
            AssetManager.AssetDeleted += AddPathToList;
            AssetManager.RefreshFinished += RecompileScripts;

            this.PluginCompiler = new PluginCompiler();

            AddReferencesForCompilerParameter(PluginCompiler);
        }
        
        private void AddReferencesForCompilerParameter(PluginCompiler PluginCompiler)
        {
            var assemblies = GetAllAssembliesInCurrentDomainDirectory();
            PluginCompiler.AddReferencedAssemblies(assemblies.Distinct().ToArray());
        }

        private IEnumerable<string> GetAllAssembliesInCurrentDomainDirectory()
        {
            foreach (var path in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory))
            {
                if (path.EndsWith(".dll") || path.EndsWith(".exe"))
                    yield return path;
            }
        }

        private void AddPathToList(string assetPath)
        {
            if (assetPath.EndsWith(FileExtensions.PluginD))
                m_ModifiedFilesSinceLastRecompilation.Add(assetPath);
        }

        private void RecompileScripts()
        {
            if (m_ModifiedFilesSinceLastRecompilation.Count == 0)
                return;

            PluginCompiler.SetOutputPath(CustomAssemblyPath);
            PluginLoader.SetInputPath(CustomAssemblyPath);
            PluginLoader.DestroyUserAppDomain();

            Profiler.Start("PluginManager_RecompileScripts");

            var scriptAssets = AssetManager.Assets.Where(a => a.Path.EndsWith(FileExtensions.PluginD));
            var scriptValues = scriptAssets.Select(a => a.Importer.Value).Where(s => s != null).Cast<string>();

            foreach (var script in scriptValues)
            {
                PluginCompiler.CompileCode(script);
            }

            PluginLoader.CreateUserAppDomain();
            PluginLoader.LoadUserAssemblies();

            Profiler.Stop("PluginManager_RecompileScripts");
            Logger.Log(LogType.Log, "Scripts successfully recompiled");

            m_ModifiedFilesSinceLastRecompilation.Clear();
        }
    }
}
