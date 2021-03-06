﻿using Robot.Abstractions;
using Robot.Settings;
using RobotRuntime;
using RobotRuntime.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Unity.Lifetime;

namespace Robot.Scripts
{
    /// <summary>
    /// ScriptManager lives in base robot assemblies. Its purpose is to communicate with asset manager and script compiler to issue the compilation process
    /// and to issue script loading.
    /// Directly communicates with ScriptCompiler and ScriptLoader from runtime.
    /// </summary>
    [RegisterTypeToContainer(typeof(IScriptManager), typeof(ContainerControlledLifetimeManager))]
    public class ScriptManager : IScriptManager
    {
        public bool AllowCompilation { get; set; } = true;
        public bool IsCompilingOrReloadingAssemblies => (m_LastCompilationTask != null) ? !m_LastCompilationTask.IsCompleted : false;

        private object CompilationLock = new object();

        private Task<bool> m_LastCompilationTask;

        private string CustomAssemblyName { get { return ScriptLoader.UserAssemblyName; } }
        private string CustomAssemblyPath { get { return ScriptLoader.UserAssemblyPath; } }

        private readonly IScriptCompiler ScriptCompiler;
        private readonly IScriptLoader ScriptLoader;
        private readonly IAssetManager AssetManager;
        private readonly ISettingsManager SettingsManager;
        public ScriptManager(IScriptCompiler ScriptCompiler, IScriptLoader ScriptLoader, IAssetManager AssetManager,
            IModifiedAssetCollector ModifiedAssetCollector, ISettingsManager SettingsManager)
        {
            this.ScriptCompiler = ScriptCompiler;
            this.ScriptLoader = ScriptLoader;
            this.AssetManager = AssetManager;
            this.SettingsManager = SettingsManager;

            ModifiedAssetCollector.ExtensionFilters.Add(FileExtensions.ScriptD);
            ModifiedAssetCollector.ExtensionFilters.Add(FileExtensions.DllD);
            ModifiedAssetCollector.AssetsModified += _ => OnAssetsModified();
            ModifiedAssetCollector.AssetsRenamed += _ => OnAssetsModified();
        }

        private void OnAssetsModified()
        {
            CompileScriptsAndReloadUserDomain();
        }

        public Task<bool> CompileScriptsAndReloadUserDomain()
        {
            lock (CompilationLock)
            {
                var ScriptAssets = AssetManager.Assets.Where(a => a.Path.EndsWith(FileExtensions.ScriptD));
                var scriptPaths = ScriptAssets.Select(a => a.Path); //.Where(s => s != null).Cast<string>();

                AddPluginAssetsAsReferencesToCompilerSettings();

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

                        //if (result)  
                        //{ 

                        // I still need to load other precompiled dlls. 
                        // CustomAssembly is not deleted so it should be fine to reload even if compilation failed
                        ScriptLoader.DestroyUserAppDomain();
                        ScriptLoader.CreateUserAppDomain();
                        //}

                        return result;
                    }
                    else
                        return false;
                });
            }
        }

        private void AddPluginAssetsAsReferencesToCompilerSettings()
        {
            var PluginPaths = AssetManager.Assets.Where(a => a.HoldsType() == typeof(Assembly))
                .Select(a => a.Path).ToArray();
            //.Select(a => Path.Combine(Environment.CurrentDirectory, a.Importer.Path)).ToArray();

            if (PluginPaths != null)
                SettingsManager.GetSettings<CompilerSettings>().CompilerReferencesFromProjectFolder = PluginPaths;
            else
                Logger.Log(LogType.Error, "PluginPaths were null. That should not be possible. Please report a bug.");
        }

        // TODO: Not used anymore. Should it be?
        private void OnScriptsRecompiled()
        {
            ScriptLoader.CreateUserAppDomain(); // This also loads the assemblies
        }
    }
}
