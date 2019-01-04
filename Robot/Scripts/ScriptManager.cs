using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Robot.Scripts
{
    /// <summary>
    /// ScriptManager lives in base robot assemblies. Its purpose is to communicate with asset manager and script compiler to issue the compilation process
    /// and to issue script loading.
    /// Directly communicates with ScriptCompiler and ScriptLoader from runtime.
    /// </summary>
    public class ScriptManager : IScriptManager
    {
        private string CustomAssemblyName { get { return "CustomAssembly.dll"; } }
        private string CustomAssemblyPath { get { return Path.Combine(Paths.MetadataPath, CustomAssemblyName); } }

        private IScriptCompiler ScriptCompiler;
        private IScriptLoader ScriptLoader;
        private IAssetManager AssetManager;
        private IModifiedAssetCollector ModifiedAssetCollector;
        public ScriptManager(IScriptCompiler ScriptCompiler, IScriptLoader ScriptLoader, IAssetManager AssetManager,
            IModifiedAssetCollector ModifiedAssetCollector)
        {
            this.ScriptCompiler = ScriptCompiler;
            this.ScriptLoader = ScriptLoader;
            this.AssetManager = AssetManager;
            this.ModifiedAssetCollector = ModifiedAssetCollector;

            ModifiedAssetCollector.ExtensionFilters.Add(FileExtensions.ScriptD);
            ModifiedAssetCollector.AssetsModified += OnAssetsModified;

            ScriptCompiler.RecordingsRecompiled += OnRecordingsRecompiled;
        }

        private void OnAssetsModified(IList<string> modifiedAssets)
        {
            CompileScriptsAndReloadUserDomain();
        }

        public void CompileScriptsAndReloadUserDomain()
        {
            ScriptCompiler.SetOutputPath(CustomAssemblyPath);

            ScriptLoader.UserAssemblyPath = CustomAssemblyPath;
            ScriptLoader.UserAssemblyName = CustomAssemblyName;
            ScriptLoader.DestroyUserAppDomain();

            var recordingAssets = AssetManager.Assets.Where(a => a.Path.EndsWith(FileExtensions.ScriptD));
            var recordingValues = recordingAssets.Select(a => a.Importer.Value).Where(s => s != null).Cast<string>();

            ScriptCompiler.CompileCode(recordingValues.ToArray());
        }

        private void OnRecordingsRecompiled()
        {
            ScriptLoader.CreateUserAppDomain(); // This also loads the assemblies
        }
    }
}
