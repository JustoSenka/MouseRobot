using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RobotRuntime.Plugins
{
    public class PluginLoader : IPluginLoader
    {
        private string DomainName { get { return "UserScripts"; } }
        public AppDomain PluginDomain { get; private set; }

        private string UserAssemblyPath { get; set; }

        private PluginDomainManager PluginDomainManager;
        public PluginLoader()
        {

        }

        public void DestroyUserAppDomain()
        {
            if (PluginDomain != null)
                AppDomain.Unload(PluginDomain);

            PluginDomain = null;
        }

        public void CreateUserAppDomain()
        {
            DestroyUserAppDomain();

            var AppDomainSetup = new AppDomainSetup() { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory };
            PluginDomain = AppDomain.CreateDomain(DomainName, AppDomain.CurrentDomain.Evidence, AppDomainSetup);

            PluginDomainManager = (PluginDomainManager)PluginDomain.CreateInstanceAndUnwrap(
                typeof(PluginDomainManager).Assembly.FullName, "RobotRuntime.Plugins.PluginDomainManager");

            var assemblyNames = GetAllAssembliesInCurrentDomainDirectory().ToArray();
            PluginDomainManager.LoadAssemblies(assemblyNames);
        }

        public void LoadUserAssemblies()
        {
            PluginDomainManager.LoadAssemblies(new[] { UserAssemblyPath }, true);
        }

        public void SetInputPath(string customAssemblyPath)
        {
            UserAssemblyPath = customAssemblyPath;
        }

        private IEnumerable<string> GetAllAssembliesInCurrentDomainDirectory()
        {
            foreach (var path in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory))
            {
                if (path.EndsWith(".dll") || path.EndsWith(".exe"))
                    yield return path;
            }
        }
    }
}
