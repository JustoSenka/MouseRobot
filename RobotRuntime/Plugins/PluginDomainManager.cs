using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity;

namespace RobotRuntime.Plugins
{
    /// <summary>
    /// This class is supposed to be in UserDomain and its purpose is to load dependencies, user assemblies and instantiate objects
    /// </summary>
    public class PluginDomainManager : MarshalByRefObject, IPluginDomainManager
    {
        public Assembly[] Assemblies;

        public PluginDomainManager()
        {

        }

        // Might not work due to UnityContainer being un-marshable
        public T ResolveInterface<T>(UnityContainer Container)
        {
            return Container.Resolve<T>();
        }

        public object Instantiate(string className)
        {
            var type = Assemblies.SelectMany(a => a.GetTypes()).First(t => t.FullName == className);
            return Activator.CreateInstance(type);
        }

        public void LoadAssemblies(string[] paths, bool userAssemblies = false)
        {
            if (userAssemblies)
                Assemblies = LoadAssemblies(paths).ToArray();
            else
                LoadAssemblies(paths);
        }

        private IEnumerable<Assembly> LoadAssemblies(string[] paths)
        {
            foreach (var path in paths)
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.LoadFrom(path);
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
