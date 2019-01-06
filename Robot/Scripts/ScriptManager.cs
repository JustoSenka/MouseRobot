using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

            ScriptCompiler.ScriptsRecompiled += OnScriptsRecompiled;
        }

        private void OnAssetsModified(IList<string> modifiedAssets)
        {
            CompileScriptsAndReloadUserDomain();
        }

        public Task<bool> CompileScriptsAndReloadUserDomain()
        {
            return Task.Run(async () =>
            {
                ScriptCompiler.SetOutputPath(CustomAssemblyPath);

                ScriptLoader.UserAssemblyPath = CustomAssemblyPath;
                ScriptLoader.UserAssemblyName = CustomAssemblyName;
                ScriptLoader.DestroyUserAppDomain();

                var ScriptAssets = AssetManager.Assets.Where(a => a.Path.EndsWith(FileExtensions.ScriptD));
                var scriptValues = ScriptAssets.Select(a => a.Importer.Value).Where(s => s != null).Cast<string>();

                return await ScriptCompiler.CompileCode(scriptValues.ToArray());
            });
        }

        private void OnScriptsRecompiled()
        {
            ScriptLoader.CreateUserAppDomain(); // This also loads the assemblies
        }
    }
}
