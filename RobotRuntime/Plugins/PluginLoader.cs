using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RobotRuntime.Plugins
{
    /// <summary>
    /// Plugin loader lives in runtime and its purpose is to create user domain and load user assemblies. 
    /// Directly comunicates with PluginDomainManager
    /// </summary>
    public class PluginLoader : IPluginLoader
    {
        private AppDomain m_PluginDomain;

        private string DomainName { get { return "UserScripts"; } }

        public string UserAssemblyName { get; set; }
        public string UserAssemblyPath { get; set; }

        public event Action UserDomainReloaded;
        public event Action UserDomainReloading;

        private PluginDomainManager PluginDomainManager;
        public PluginLoader()
        {
            // TODO: runtime on its own should create domain and load assemblies
            // at this moment cannot accomplish, since path is not known by itself
            // Should runtime have its own manager, since released test paths might be completely different
        }

        public void DestroyUserAppDomain()
        {
            UserDomainReloading?.Invoke();

            if (m_PluginDomain != null)
                AppDomain.Unload(m_PluginDomain);

            m_PluginDomain = null;
        }

        public void CreateUserAppDomain()
        {
            DestroyUserAppDomain();

            var AppDomainSetup = new AppDomainSetup();
            AppDomainSetup.ApplicationName = DateTime.Now.ToString("hh:MM:ss:ffff");
            AppDomainSetup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            AppDomainSetup.PrivateBinPath = AppDomain.CurrentDomain.BaseDirectory;

            m_PluginDomain = AppDomain.CreateDomain(DomainName, AppDomain.CurrentDomain.Evidence, AppDomainSetup);

            PluginDomainManager = (PluginDomainManager)m_PluginDomain.CreateInstanceAndUnwrap(
                typeof(PluginDomainManager).Assembly.FullName, "RobotRuntime.Plugins.PluginDomainManager");

            var assemblyNames = AppDomain.CurrentDomain.GetAllAssembliesInBaseDirectory().ToArray();
            PluginDomainManager.LoadAssemblies(assemblyNames);

            LoadUserAssemblies(); // User assemblies must be loaded before UserDomainReloaded event fires

            UserDomainReloaded?.Invoke();
        }

        public void LoadUserAssemblies()
        {
            PluginDomainManager.LoadAssemblies(new[] { UserAssemblyPath }, true);
        }

        public IEnumerable<T> IterateUserAssemblies<T>(Func<Assembly, T> func)
        {
            throw new NotImplementedException();
        }
    }
}
