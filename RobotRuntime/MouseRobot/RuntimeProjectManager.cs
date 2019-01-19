using RobotRuntime.Abstractions;
using System;
using System.IO;

namespace RobotRuntime
{
    public class RuntimeProjectManager : IRuntimeProjectManager
    {
        public string ProjectName => Path.GetFileName(Environment.CurrentDirectory);
        public event Action<string> NewProjectOpened;

        private readonly IAssetGuidManager AssetGuidManager;
        public RuntimeProjectManager(IAssetGuidManager AssetGuidManager)
        {
            this.AssetGuidManager = AssetGuidManager;
        }

        public virtual void InitProject(string path)
        {
            Environment.CurrentDirectory = path;

            AssetGuidManager.LoadMetaFiles();

            NewProjectOpened?.Invoke(path);
        }

        protected virtual void OnNewProjectOpened(string path)
        {
            NewProjectOpened?.Invoke(path);
        }
    }
}
