using RobotRuntime.Plugins;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RobotRuntime.Abstractions
{
    public interface IPluginLoader
    {
        string UserAssemblyName { get; set; }
        string UserAssemblyPath { get; set; }

        void DestroyUserAppDomain();
        void CreateUserAppDomain();
        void LoadUserAssemblies();

        IEnumerable<T> IterateUserAssemblies<T>(Func<Assembly, T> func);

        event Action UserDomainReloaded;
    }
}