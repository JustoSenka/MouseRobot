using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace RobotRuntime.Scripts
{
    /// <summary>
    /// Script loader lives in runtime and its purpose is to create user domain and load user assemblies. 
    /// Directly comunicates with ScriptDomainManager
    /// </summary>
    public class ScriptLoader : IScriptLoader
    {
        private AppDomain m_ScriptDomain;

        private string DomainName { get { return "UserRecordings"; } }

        public string UserAssemblyName { get; set; }
        public string UserAssemblyPath { get; set; }

        public event Action UserDomainReloaded;
        public event Action UserDomainReloading;

        private ScriptDomainManager ScriptDomainManager;
        public ScriptLoader()
        {
            // TODO: runtime on its own should create domain and load assemblies
            // at this moment cannot accomplish, since path is not known by itself
            // Should runtime have its own manager, since released test paths might be completely different
        }

        public void DestroyUserAppDomain()
        {
            UserDomainReloading?.Invoke();

            if (m_ScriptDomain != null)
                AppDomain.Unload(m_ScriptDomain);

            m_ScriptDomain = null;
        }

        public void CreateUserAppDomain()
        {
            DestroyUserAppDomain();

            var AppDomainSetup = new AppDomainSetup();
            AppDomainSetup.ApplicationName = DateTime.Now.ToString("hh:MM:ss:ffff");
            AppDomainSetup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            AppDomainSetup.PrivateBinPath = AppDomain.CurrentDomain.BaseDirectory;

            m_ScriptDomain = AppDomain.CreateDomain(DomainName, AppDomain.CurrentDomain.Evidence, AppDomainSetup);

            ScriptDomainManager = (ScriptDomainManager)m_ScriptDomain.CreateInstanceAndUnwrap(
                typeof(ScriptDomainManager).Assembly.FullName, "RobotRuntime.Scripts.ScriptDomainManager");

            var assemblyNames = AppDomain.CurrentDomain.GetAllAssembliesInBaseDirectory().ToArray();
            ScriptDomainManager.LoadAssemblies(assemblyNames);

            LoadUserAssemblies(); // User assemblies must be loaded before UserDomainReloaded event fires

            UserDomainReloaded?.Invoke();
        }

        public void LoadUserAssemblies()
        {
            ScriptDomainManager.LoadAssemblies(new[] { UserAssemblyPath }, true);
        }

        public IEnumerable<T> IterateUserAssemblies<T>(Func<Assembly, T> func)
        {
            throw new NotImplementedException();
        }
    }
}
