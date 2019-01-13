using RobotRuntime.Abstractions;
using System;
using System.IO;

namespace RobotRuntime
{
    public class RuntimeProjectManager : IRuntimeProjectManager
    {
        public string ProjectName => Path.GetFileName(Environment.CurrentDirectory);
        public event Action<string> NewProjectOpened;

        public RuntimeProjectManager()
        {
        }

        public virtual void InitProject(string path)
        {
            Environment.CurrentDirectory = path;

            // Runtime ??? AssetManager.Refresh();

            NewProjectOpened?.Invoke(path);
        }

        protected virtual void OnNewProjectOpened(string path)
        {
            NewProjectOpened?.Invoke(path);
        }
    }
}
