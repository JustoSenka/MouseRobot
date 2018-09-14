using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel;

namespace RobotRuntime.Plugins
{
    public class PluginLoaderNoDomain : IPluginLoader
    {
        public AsyncOperation AsyncOperationOnUI { private get; set; }

        public string UserAssemblyName { get; set; }
        public string UserAssemblyPath { get; set; }

        public event Action UserDomainReloaded;
        public event Action UserDomainReloading;

        private Assembly[] Assemblies;

        private IProfiler Profiler;
        public PluginLoaderNoDomain(IProfiler Profiler)
        {
            this.Profiler = Profiler;
            // TODO: Runtime
        }

        public void DestroyUserAppDomain()
        {
            UserDomainReloading?.Invoke();
            //AsyncOperationOnUI?.Post(() => UserDomainReloading?.Invoke());
            Assemblies = null;
        }

        public void CreateUserAppDomain()
        {
            Profiler.Start("PluginLoader.ReloadAppDomain");

            if (Assemblies != null)
                DestroyUserAppDomain();

            LoadUserAssemblies();
            UserDomainReloaded?.Invoke();
            // AsyncOperationOnUI?.Post(() => UserDomainReloaded?.Invoke());

            Profiler.Stop("PluginLoader.ReloadAppDomain");
        }

        public void LoadUserAssemblies()
        {
            Assemblies = LoadAssemblies(new[] { UserAssemblyPath }).ToArray();
        }

        public IEnumerable<T> IterateUserAssemblies<T>(Func<Assembly, T> func)
        {
            if (Assemblies == null)
                yield break;

            foreach (var assembly in Assemblies)
                yield return func(assembly);
        }

        private IEnumerable<Assembly> LoadAssemblies(string[] paths)
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
                    yield return assembly;
            }
        }
    }
}

