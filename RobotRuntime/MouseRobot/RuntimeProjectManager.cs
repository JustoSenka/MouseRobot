using RobotRuntime.Abstractions;
using System;
using System.IO;
using System.Threading.Tasks;
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

        public virtual async Task InitProject(string path)
        {
            Environment.CurrentDirectory = path;

            ScriptLoader.CreateUserAppDomain();

            NewProjectOpened?.Invoke(path);

            await Task.CompletedTask;
        }

        protected virtual void OnNewProjectOpened(string path)
        {
            NewProjectOpened?.Invoke(path);
        }
    }
}
