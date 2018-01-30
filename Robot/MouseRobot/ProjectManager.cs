using Robot.Abstractions;
using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using System;
using System.IO;

namespace Robot
{
    public class ProjectManager : IProjectManager
    {
        private IAssetManager AssetManager;
        private IAssetGuidManager AssetGuidManager;
        public ProjectManager(IAssetManager AssetManager, IAssetGuidManager AssetGuidManager)
        {
            this.AssetManager = AssetManager;
            this.AssetGuidManager = AssetGuidManager;
        }

        public void InitProject(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            Environment.CurrentDirectory = path;

            foreach (var projPath in Paths.PathArray)
            {
                if (!Directory.Exists(projPath))
                    Directory.CreateDirectory(projPath);
            }

            AssetGuidManager.LoadMetaFiles();
            AssetManager.Refresh();
        }

        public bool IsPathAProject(string path)
        {
            if (!Directory.Exists(path))
                return false;

            foreach (var projPath in Paths.PathArray)
            {
                var folder = Paths.GetFolder(projPath);
                if (!Directory.Exists(Path.Combine(path, folder)))
                    return false;
            }
            return true;
        }
    }
}
