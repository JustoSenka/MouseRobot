using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RobotRuntime.Plugins
{
    /// <summary>
    /// This class is supposed to be in UserDomain and its purpose is to load dependencies, user assemblies
    /// </summary>
    public class PluginDomainManager : MarshalByRefObject, IPluginDomainManager
    {
        public Assembly[] Assemblies;

        public PluginDomainManager()
        {

        }

        public string[] GetAllTypeNamesWhichImplementInterface(Type type)
        {
            var types = Assemblies.GetAllTypesWhichImplementInterface(type);
            return types.Select(t => t.FullName).ToArray();
        }

        /// <summary>
        /// This will not work if called from different app domain.
        /// </summary>
        public Type[] GetAllTypesWhichImplementInterface(Type type)
        {
            var types = Assemblies.GetAllTypesWhichImplementInterface(type);
            return types.ToArray();
        }

        public object Instantiate(string FullTypeName)
        {
            var type = Assemblies.SelectMany(a => a.GetTypes()).First(t => t.FullName == FullTypeName);

            //var handle = Activator.CreateInstanceFrom(Assemblies[0].Location, FullTypeName);
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
