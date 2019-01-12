using System;
using System.Collections.Generic;
using System.Reflection;

namespace RobotRuntime.Abstractions
{
    public interface IScriptLoader
    {
        string UserAssemblyName { get; }
        string UserAssemblyPath { get; }

        void DestroyUserAppDomain();
        void CreateUserAppDomain();
        void LoadUserAssemblies();

        IEnumerable<T> IterateUserAssemblies<T>(Func<Assembly, T> func);

        event Action UserDomainReloaded;
        event Action UserDomainReloading;
    }
}