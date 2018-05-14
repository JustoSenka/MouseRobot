using RobotRuntime.Plugins;
using System;

namespace RobotRuntime.Abstractions
{
    public interface IPluginLoader
    {
        string UserAssemblyName { get; set; }
        string UserAssemblyPath { get; set; }

        PluginDomainManager PluginDomainManager { get; }
        
        void DestroyUserAppDomain();
        void CreateUserAppDomain();
        void LoadUserAssemblies();

        event Action UserDomainReloaded;
    }
}