using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using System.Collections.Generic;
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
        public bool IsCompilingOrReloadingAssemblies => (m_LastCompilationTask != null) ? !m_LastCompilationTask.IsCompleted : false;

        private object CompilationLock = new object();

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
            lock (CompilationLock)
            {
                var ScriptAssets = AssetManager.Assets.Where(a => a.Path.EndsWith(FileExtensions.ScriptD));
                var scriptPaths = ScriptAssets.Select(a => a.Importer.Path); //.Where(s => s != null).Cast<string>();

                if (IsCompilingOrReloadingAssemblies)
                {
                    ScriptCompiler.UpdateCompilationSources(scriptPaths.ToArray());

                    // Always return the same task and never create additional tasks, or multiple domain reloads will happen after one compilation
                    // because script compiler also returns always the same task for compilation
                    return m_LastCompilationTask;
                }

                return m_LastCompilationTask = Task.Run(async () =>
                {
                    if (AllowCompilation)
                    {
                        ScriptCompiler.SetOutputPath(CustomAssemblyPath);

                        var result = await ScriptCompiler.CompileCode(scriptPaths.ToArray());

                        if (result)
                        {
                            ScriptLoader.DestroyUserAppDomain();
                            ScriptLoader.CreateUserAppDomain();
                        }

                        return result;
                    }
                    else
                        return false;
                });
            }
        }

        // TODO: Not used anymore. Should it be?
        private void OnScriptsRecompiled()
        {
            ScriptLoader.CreateUserAppDomain(); // This also loads the assemblies
        }
    }
}
