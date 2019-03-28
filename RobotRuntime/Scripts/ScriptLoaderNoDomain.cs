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

        private Dictionary<string, Assembly> m_Assemblies = new Dictionary<string, Assembly>(StringComparer.InvariantCultureIgnoreCase);
        private readonly object CreateUserDomainLock = new object();

        private IRuntimeAssetManager RuntimeAssetManager;
        private IProfiler Profiler;
        private IRuntimeProjectManager ProjectManager;
        public ScriptLoaderNoDomain(IRuntimeAssetManager RuntimeAssetManager, IProfiler Profiler, IRuntimeProjectManager ProjectManager)
        {
            this.RuntimeAssetManager = RuntimeAssetManager;
            this.Profiler = Profiler;
            this.ProjectManager = ProjectManager;

            ProjectManager.NewProjectOpened += OnNewProjectOpened;

            // This one is needed. Since my plugins are loaded from bytes and they don't seem to load dependencies by path correctly
            // Since I'm loading them manually, I can return correct dependency from already loaded ones
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        /// <summary>
        /// If dependency for dll is not found, will try to look for already loaded plugins and return correct one.
        /// This might happen due to the way I load plugins. https://weblog.west-wind.com/posts/2016/dec/12/loading-net-assemblies-out-of-seperate-folders
        /// </summary>
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Ignore missing resources
            if (args.Name.Contains(".resources"))
                return null;

            var asm = m_Assemblies.Select(k => k.Value).FirstOrDefault(a => a.FullName.Equals(args.Name, StringComparison.InvariantCultureIgnoreCase));
            if (asm == default)
                Logger.Log(LogType.Error, "Cannot resolve assembly not found problem. Was looking for: " + args.Name);

            return asm;
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
            if (m_Assemblies.Count != 0)
                DestroyUserAppDomain();

            lock (CreateUserDomainLock)
            {
                Profiler.Start("ScriptLoader.ReloadAppDomain");

                LoadUserAssemblies();
                UserDomainReloaded?.Invoke();

                Logger.Log(LogType.Log, "User assemblies successfully loaded.");

                Profiler.Stop("ScriptLoader.ReloadAppDomain");
            }
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
                LoadAssemblies(m_Assemblies, true, userAssemblies);
                LoadAssemblies(m_Assemblies, false, userPlugins);
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

        private static void LoadAssemblies(Dictionary<string, Assembly> dict, bool loadPdbFiles, string[] paths)
        {
            foreach (var path in paths)
            {
                Assembly assembly = null;
                try
                {
                    // Get .pdb file path from dll path
                    var fileName = Path.GetFileNameWithoutExtension(path);
                    var dir = Path.GetDirectoryName(path);
                    var pdbPath = Path.Combine(dir, fileName) + ".pdb";

                    // This is done in this particular way so we do not keep the file locked
                    var dllBytes = File.ReadAllBytes(path);
                    if (File.Exists(pdbPath) && loadPdbFiles)
                    {
                        var pdbBytes = File.ReadAllBytes(pdbPath);
                        assembly = Assembly.Load(dllBytes, pdbBytes);
                    }
                    else
                    {
                        if (loadPdbFiles)
                            Logger.Log(LogType.Warning, "Cannot find debug symbols .pdb for assembly: " + path);

                        assembly = Assembly.Load(dllBytes);
                    }
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

