using Robot.Abstractions;
using RobotRuntime.Abstractions;
using RobotRuntime.IO;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Robot
{
    public class ProjectManager : IProjectManager
    {
        public string ProjectName => Path.GetFileName(Environment.CurrentDirectory);

        public IList<string> LastKnownProjectPaths { get; private set; }

        public event Action<string> NewProjectOpened;

        private ObjectIO m_Serializer;

        private const string k_FileName = "LastKnownProjectPaths.config";
        private string FilePath { get { return Path.Combine(Paths.RoamingAppdataPath, k_FileName); } }

        private IAssetManager AssetManager;
        private IAssetGuidManager AssetGuidManager;
        public ProjectManager(IAssetManager AssetManager, IAssetGuidManager AssetGuidManager)
        {
            this.AssetManager = AssetManager;
            this.AssetGuidManager = AssetGuidManager;

            m_Serializer = new YamlDotNetIO();

            RestoreAndRemovePathsOfDeletedProjects();
        }

        public void RestoreAndRemovePathsOfDeletedProjects()
        {
            RestoreSettings();

            foreach (var path in LastKnownProjectPaths.ToArray())
            {
                if (!IsPathAProject(path))
                    LastKnownProjectPaths.Remove(path);
            }
            SaveSettings();
        }

        public void InitProject(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            Environment.CurrentDirectory = path;

            foreach (var projPath in Paths.ProjectPathArray)
            {
                if (!Directory.Exists(projPath))
                    Directory.CreateDirectory(projPath);
            }

            RememberPathInSettings(path);

            AssetGuidManager.LoadMetaFiles();
            AssetManager.Refresh();

            NewProjectOpened?.Invoke(path);
        }

        public bool IsPathAProject(string path)
        {
            if (!Directory.Exists(path))
                return false;

            foreach (var projPath in Paths.ProjectPathArray)
            {
                var folder = Path.GetFileName(projPath);
                if (!Directory.Exists(Path.Combine(path, folder)))
                    return false;
            }
            return true;
        }

        public void RestoreSettings()
        {
            var lastPaths = m_Serializer.LoadObject<string[]>(FilePath);

            if (lastPaths != null)
                LastKnownProjectPaths = lastPaths.ToList();

            if (LastKnownProjectPaths == null)
                LastKnownProjectPaths = new List<string>();
        }

        public void SaveSettings()
        {
            if (LastKnownProjectPaths != null)
                m_Serializer.SaveObject(FilePath, LastKnownProjectPaths.ToArray());
        }

        public void RememberPathInSettings(string path)
        {
            RestoreSettings();

            if (LastKnownProjectPaths.Contains(path))
                LastKnownProjectPaths.Remove(path);

            LastKnownProjectPaths.Insert(0, path);
            SaveSettings();
        }
    }
}
