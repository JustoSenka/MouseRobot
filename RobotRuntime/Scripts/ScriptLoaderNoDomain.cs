using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RobotRuntime.Scripts
{
    public class ScriptLoaderNoDomain : IScriptLoader
    {
        public string UserAssemblyName { get { return "CustomAssembly.dll"; } }
        public string UserAssemblyPath { get { return Path.Combine(Paths.MetadataPath, UserAssemblyName); } }

        public event Action UserDomainReloaded;
        public event Action UserDomainReloading;

        private Dictionary<string, Assembly> m_Assemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

        private IRuntimeAssetManager RuntimeAssetManager;
        private IProfiler Profiler;
        private IRuntimeProjectManager ProjectManager;
        public ScriptLoaderNoDomain(IRuntimeAssetManager RuntimeAssetManager, IProfiler Profiler, IRuntimeProjectManager ProjectManager)
        {
            this.RuntimeAssetManager = RuntimeAssetManager;
            this.Profiler = Profiler;
            this.ProjectManager = ProjectManager;

            ProjectManager.NewProjectOpened += OnNewProjectOpened;
        }

        private void OnNewProjectOpened(string obj)
        {
            CreateUserAppDomain();
        }

        public void DestroyUserAppDomain()
        {
            UserDomainReloading?.Invoke();

            lock (m_Assemblies)
            {
                m_Assemblies.Clear();
            }
        }

        public void CreateUserAppDomain()
        {
            // TODO: Lock here to prevent race? Lock on DestroyUserDomain also?
            Profiler.Start("ScriptLoader.ReloadAppDomain");

            if (m_Assemblies.Count != 0)
                DestroyUserAppDomain();

            LoadUserAssemblies();
            UserDomainReloaded?.Invoke();

            Logger.Log(LogType.Log, "User assemblies successfully loaded.");

            Profiler.Stop("ScriptLoader.ReloadAppDomain");
        }

        public void LoadUserAssemblies()
        {
            // TODO: Currently it is slow and should be fixed somehow.
            RuntimeAssetManager.CollectAllImporters();

            var userAssemblies = File.Exists(UserAssemblyPath) ? new[] { UserAssemblyPath } : new string[0];
            var userPlugins = RuntimeAssetManager.AssetImporters.
                Where(i => i.HoldsType() == typeof(Assembly)).Select(i => i.Path).ToArray();

            lock (m_Assemblies)
            {
                LoadAssemblies(m_Assemblies, userAssemblies);
                LoadAssemblies(m_Assemblies, userPlugins);
            }
        }

        public IEnumerable<T> IterateUserAssemblies<T>(Func<Assembly, T> func)
        {
            // Returning new list as copy, so locking mechanism works.
            // Otherwise it is possible that new assemblies are added while somebody is iterating m_Assemblies at that moment
            // If it happens to be too slow, code using this system should make sure to lock onto some public new object(). not needed atm
            lock (m_Assemblies)
            {
                return new List<T>(m_Assemblies.Select(pair => func(pair.Value)));
            }
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

