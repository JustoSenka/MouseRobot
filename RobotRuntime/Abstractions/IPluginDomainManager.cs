﻿using Unity;

namespace RobotRuntime.Abstractions
{
    public interface IPluginDomainManager
    {
        object Instantiate(string className);
        void LoadAssemblies(string[] paths, bool userAssemblies = false);
        T ResolveInterface<T>(UnityContainer Container);
    }
}