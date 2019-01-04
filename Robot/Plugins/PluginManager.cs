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
        private string CustomAssemblyName { get { return "CustomAssembly.dll"; } }
        private string CustomAssemblyPath { get { return Path.Combine(Paths.MetadataPath, CustomAssemblyName); } }

        private IPluginCompiler PluginCompiler;
        private IPluginLoader PluginLoader;
        private IAssetManager AssetManager;
        private IModifiedAssetCollector ModifiedAssetCollector;
        public PluginManager(IPluginCompiler PluginCompiler, IPluginLoader PluginLoader, IAssetManager AssetManager,
            IModifiedAssetCollector ModifiedAssetCollector)
        {
            this.PluginCompiler = PluginCompiler;
            this.PluginLoader = PluginLoader;
            this.AssetManager = AssetManager;
            this.ModifiedAssetCollector = ModifiedAssetCollector;

            ModifiedAssetCollector.ExtensionFilters.Add(FileExtensions.PluginD);
            ModifiedAssetCollector.AssetsModified += OnAssetsModified;

            PluginCompiler.RecordingsRecompiled += OnRecordingsRecompiled;
        }

        private void OnAssetsModified(IList<string> modifiedAssets)
        {
            CompileRecordingsAndReloadUserDomain();
        }

        public void CompileRecordingsAndReloadUserDomain()
        {
            PluginCompiler.SetOutputPath(CustomAssemblyPath);

            PluginLoader.UserAssemblyPath = CustomAssemblyPath;
            PluginLoader.UserAssemblyName = CustomAssemblyName;
            PluginLoader.DestroyUserAppDomain();

            var recordingAssets = AssetManager.Assets.Where(a => a.Path.EndsWith(FileExtensions.PluginD));
            var recordingValues = recordingAssets.Select(a => a.Importer.Value).Where(s => s != null).Cast<string>();

            PluginCompiler.CompileCode(recordingValues.ToArray());
        }

        private void OnRecordingsRecompiled()
        {
            PluginLoader.CreateUserAppDomain(); // This also loads the assemblies
        }
    }
}
