using RobotRuntime.Abstractions;
using System;
using System.IO;
using Unity.Lifetime;

namespace RobotRuntime
{
    [RegisterTypeToContainer(typeof(IRuntimeProjectManager), typeof(ContainerControlledLifetimeManager))]
    public class RuntimeProjectManager : IRuntimeProjectManager
    {
        public string ProjectName => Path.GetFileName(Environment.CurrentDirectory);
        public event Action<string> NewProjectOpened;

        private readonly IScriptLoader ScriptLoader;
        public RuntimeProjectManager(IScriptLoader ScriptLoader)
        {
            this.ScriptLoader = ScriptLoader;
        }

        public virtual void InitProject(string path)
        {
            Environment.CurrentDirectory = path;

            ScriptLoader.CreateUserAppDomain();

            NewProjectOpened?.Invoke(path);
        }

        protected virtual void OnNewProjectOpened(string path)
        {
            NewProjectOpened?.Invoke(path);
        }
    }
}
