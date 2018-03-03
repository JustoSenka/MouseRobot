using System;

namespace RobotRuntime.Abstractions
{
    public interface IPluginLoader
    {
        AppDomain PluginDomain { get; }

        void DestroyUserAppDomain();
        void CreateUserAppDomain();
        void LoadUserAssemblies();
        void SetInputPath(string customAssemblyPath);
    }
}