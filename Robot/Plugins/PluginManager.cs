using Microsoft.CSharp;
using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Robot.Plugins
{
    public class PluginManager
    {
        private IList<string> m_ModifiedFilesSinceLastRecompilation = new List<string>();

        private string DomainName { get { return "UserScripts"; } }

        private string CustomAssemblyName { get { return "CustomAssembly.dll"; } }
        private string CustomAssemblyPath { get { return Path.Combine(Paths.MetadataPath, CustomAssemblyName); } }

        public AppDomain PluginDomain { get; private set; }

        private PluginCompiler PluginCompiler;
        private IAssetManager AssetManager;
        private IProfiler Profiler;
        public PluginManager(IAssetManager AssetManager, IProfiler Profiler)
        {
            this.AssetManager = AssetManager;
            this.Profiler = Profiler;

            AssetManager.AssetCreated += AddPathToList;
            AssetManager.AssetUpdated += AddPathToList;
            AssetManager.AssetDeleted += AddPathToList;
            AssetManager.RefreshFinished += RecompileScripts;

            var AppDomainSetup = new AppDomainSetup() { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory };
            PluginDomain = AppDomain.CreateDomain(DomainName, AppDomain.CurrentDomain.Evidence, AppDomainSetup);

            var proxy = (Proxy) PluginDomain.CreateInstanceAndUnwrap(typeof(Proxy).Assembly.FullName, "Proxy");
            proxy.LoadAssemblies(GetAllAssembliesInCurrentDomainDirectory().ToArray());

            this.PluginCompiler = (PluginCompiler)PluginDomain.CreateInstanceAndUnwrap(typeof(PluginCompiler).Assembly.FullName, "PluginCompiler");
            PluginCompiler.Logger = Logger.Instance;

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

            PluginCompiler.SetOutputDirectory(CustomAssemblyPath);

            Profiler.Start("PluginManager_RecompileScripts");

            var scriptAssets = AssetManager.Assets.Where(a => a.Path.EndsWith(FileExtensions.PluginD));
            var scriptValues = scriptAssets.Select(a => a.Importer.Value).Where(s => s != null).Cast<string>();

            foreach (var script in scriptValues)
            {
                PluginCompiler.CompileCode(script);
            }

            Profiler.Stop("PluginManager_RecompileScripts");
            Logger.Log(LogType.Log, "Scripts successfully recompiled");

            m_ModifiedFilesSinceLastRecompilation.Clear();
        }
    }
}
