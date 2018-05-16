using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RobotRuntime.Plugins
{
    public class PluginLoaderNoDomain : IPluginLoader
    {
        public string UserAssemblyName { get; set; }
        public string UserAssemblyPath { get; set; }

        public event Action UserDomainReloaded;

        private Assembly[] Assemblies;
        public PluginLoaderNoDomain()
        {
            // TODO: Runtime
        }

        public void DestroyUserAppDomain()
        {
            Assemblies = null;
        }

        public void CreateUserAppDomain()
        {
            DestroyUserAppDomain();
            LoadUserAssemblies();
            UserDomainReloaded?.Invoke();
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

