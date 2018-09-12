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
            var assemblies = AppDomain.CurrentDomain.GetAllAssembliesInBaseDirectory();
            PluginCompiler.AddReferencedAssemblies(assemblies.Distinct().ToArray());
        }
        
        private void AddPathToList(string assetPath)
        {
            if (assetPath.EndsWith(FileExtensions.PluginD))
                m_ModifiedFilesSinceLastRecompilation.Add(assetPath);

            // If asset manager is not set to batch asset editing mode, that means no refresh will be called,
            // but something has already changed from within app. Call refresh callback manually.
            if (!AssetManager.IsEditingAssets)
                OnAssetRefreshFinished();
        }

        private void OnAssetRefreshFinished()
        {
            if (m_ModifiedFilesSinceLastRecompilation.Count == 0)
                return;

            CompileScriptsAndReloadUserDomain();
        }

        public void CompileScriptsAndReloadUserDomain()
        {
            PluginCompiler.SetOutputPath(CustomAssemblyPath);

            PluginLoader.UserAssemblyPath = CustomAssemblyPath;
            PluginLoader.UserAssemblyName = CustomAssemblyName;
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
