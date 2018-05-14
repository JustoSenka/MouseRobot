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
    /// <summary>
    /// PluginManager lives in base robot assemblies. Its purpose is to communicate with asset manager and plugin compiler to issue the compilation process
    /// and to issue plugin loading.
    /// Directly communicates with PluginCompiler and PluginLoader from runtime.
    /// </summary>
    public class PluginManager : IPluginManager
    {
        private IList<string> m_ModifiedFilesSinceLastRecompilation = new List<string>();

        private string CustomAssemblyName { get { return "CustomAssembly.dll"; } }
        private string CustomAssemblyPath { get { return Path.Combine(Paths.MetadataPath, CustomAssemblyName); } }

        private IPluginCompiler PluginCompiler;
        private IPluginLoader PluginLoader;
        private IAssetManager AssetManager;
        public PluginManager(IPluginCompiler PluginCompiler, IPluginLoader PluginLoader, IAssetManager AssetManager)
        {
            this.PluginCompiler = PluginCompiler;
            this.PluginLoader = PluginLoader;
            this.AssetManager = AssetManager;

            AssetManager.AssetCreated += AddPathToList;
            AssetManager.AssetUpdated += AddPathToList;
            AssetManager.AssetDeleted += AddPathToList;
            AssetManager.RefreshFinished += OnAssetRefreshFinished;

            PluginCompiler.ScriptsRecompiled += OnScriptsRecompiled;

            AddReferencesForCompilerParameter();
        }

        private void AddReferencesForCompilerParameter()
        {
            var assemblies = GetAllAssembliesInCurrentDomainDirectory();
            PluginCompiler.AddReferencedAssemblies(assemblies.Distinct().ToArray());
        }

        // TODO: Duplicated in PluginLoader
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

        private void OnAssetRefreshFinished()
        {
            if (m_ModifiedFilesSinceLastRecompilation.Count == 0)
                return;

            RecompileScripts();
        }

        private void RecompileScripts()
        {
            PluginCompiler.SetOutputPath(CustomAssemblyPath);

            PluginLoader.UserAssemblyPath = CustomAssemblyPath;
            PluginLoader.UserAssemblyName= CustomAssemblyName;
            PluginLoader.DestroyUserAppDomain();

            var scriptAssets = AssetManager.Assets.Where(a => a.Path.EndsWith(FileExtensions.PluginD));
            var scriptValues = scriptAssets.Select(a => a.Importer.Value).Where(s => s != null).Cast<string>();

            PluginCompiler.CompileCode(scriptValues.ToArray());
        }

        private void OnScriptsRecompiled()
        {
            PluginLoader.CreateUserAppDomain();
            //PluginLoader.LoadUserAssemblies();

            m_ModifiedFilesSinceLastRecompilation.Clear();
        }
    }
}
