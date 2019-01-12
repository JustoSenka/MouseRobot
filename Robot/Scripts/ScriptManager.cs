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
        public bool AllowCompilation { get; set; } = true;
        public bool IsCompilingOrReloadingAssemblies { get; private set; }

        private Task<bool> m_LastCompilationTask;

        private string CustomAssemblyName { get { return ScriptLoader.UserAssemblyName; } }
        private string CustomAssemblyPath { get { return ScriptLoader.UserAssemblyPath; } }

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
        }

        private void OnAssetsModified(IList<string> modifiedAssets)
        {
            CompileScriptsAndReloadUserDomain();
        }

        public Task<bool> CompileScriptsAndReloadUserDomain()
        {
            var ScriptAssets = AssetManager.Assets.Where(a => a.Path.EndsWith(FileExtensions.ScriptD));
            var scriptValues = ScriptAssets.Select(a => a.Importer.Value).Where(s => s != null).Cast<string>();

            if (IsCompilingOrReloadingAssemblies)
            {
                ScriptCompiler.UpdateCompilationSources(scriptValues.ToArray());

                // Always return the same task and never create additional tasks, or multiple domain reloads will happen after one compilation
                // because script compiler also returns always the same task for compilation
                return m_LastCompilationTask;
            }

            return m_LastCompilationTask = Task.Run(() =>
            {
                if (AllowCompilation)
                {
                    IsCompilingOrReloadingAssemblies = true;

                    ScriptCompiler.SetOutputPath(CustomAssemblyPath);

                    var result = ScriptCompiler.CompileCode(scriptValues.ToArray()).Result;

                    if (result)
                    {
                        ScriptLoader.DestroyUserAppDomain();
                        ScriptLoader.CreateUserAppDomain();
                    }

                    IsCompilingOrReloadingAssemblies = false;
                    return result;
                }
                else
                    return false;
            });
        }

        // TODO: Not used anymore. Should it be?
        private void OnScriptsRecompiled()
        {
            ScriptLoader.CreateUserAppDomain(); // This also loads the assemblies
        }
    }
}
