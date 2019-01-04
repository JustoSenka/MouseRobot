using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel;

namespace RobotRuntime.Scripts
{
    public class ScriptLoaderNoDomain : IScriptLoader
    {
        public string UserAssemblyName { get; set; }
        public string UserAssemblyPath { get; set; }

        public event Action UserDomainReloaded;
        public event Action UserDomainReloading;

        private Dictionary<string, Assembly> m_Assemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

        private IRuntimeAssetManager RuntimeAssetManager;
        private IProfiler Profiler;
        public ScriptLoaderNoDomain(IRuntimeAssetManager RuntimeAssetManager, IProfiler Profiler)
        {
            this.RuntimeAssetManager = RuntimeAssetManager;
            this.Profiler = Profiler;
        }

        public void DestroyUserAppDomain()
        {
            UserDomainReloading?.Invoke();
            m_Assemblies.Clear();
        }

        public void CreateUserAppDomain()
        {
            Profiler.Start("ScriptLoader.ReloadAppDomain");

            if (m_Assemblies.Count != 0)
                DestroyUserAppDomain();

            LoadUserAssemblies();
            UserDomainReloaded?.Invoke();

            Profiler.Stop("ScriptLoader.ReloadAppDomain");
        }

        public void LoadUserAssemblies()
        {
            // TODO: Currently it is slow and should be fixed somehow.
            RuntimeAssetManager.CollectAllImporters();

            var userAssemblies = new[] { UserAssemblyPath };
            var userPlugins = RuntimeAssetManager.AssetImporters.
                Where(i => i.HoldsType() == typeof(Assembly)).Select(i => i.Path).ToArray();

            LoadAssemblies(m_Assemblies, userAssemblies);
            LoadAssemblies(m_Assemblies, userPlugins);
        }

        public IEnumerable<T> IterateUserAssemblies<T>(Func<Assembly, T> func)
        {
            if (m_Assemblies.Count == 0)
                yield break;

            foreach (var pair in m_Assemblies)
                yield return func(pair.Value);
        }

        private static void LoadAssemblies(Dictionary<string, Assembly> dict, string[] paths)
        {
            foreach (var path in paths)
            {
                Assembly assembly = null;
                try
                {
                    // This is done in this particular way so we do not keep the file locked
                    var bytes = File.ReadAllBytes(path);
                    assembly = Assembly.Load(bytes);
                }
                catch (Exception)
                {
                    Console.WriteLine("Assembly could not be loaded: " + path);
                }

                if (assembly != null)
                    dict.AddOrOverride(path, assembly);
            }
        }
    }
}

