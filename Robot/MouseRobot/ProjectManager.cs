using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.IO;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.Lifetime;

namespace Robot
{
    [RegisterTypeToContainer(typeof(IProjectManager), typeof(ContainerControlledLifetimeManager))]
    [RegisterTypeToContainer(typeof(IRuntimeProjectManager), typeof(ContainerControlledLifetimeManager))]
    public class ProjectManager : RuntimeProjectManager, IProjectManager, IRuntimeProjectManager
    {
        public IList<string> LastKnownProjectPaths { get; private set; }

        private ObjectIO m_Serializer = new JsonObjectIO();

        private const string k_FileName = "LastKnownProjectPaths.config";
        private string FilePath { get { return Path.Combine(Paths.RoamingAppdataPath, k_FileName); } }

        private readonly IAssetManager AssetManager;
        private readonly IScriptManager ScriptManager;
        private readonly ISettingsManager SettingsManager;
        public ProjectManager(IAssetManager AssetManager, IScriptManager ScriptManager, ISettingsManager SettingsManager, IScriptLoader ScriptLoader) : base(ScriptLoader)
        {
            this.AssetManager = AssetManager;
            this.ScriptManager = ScriptManager;
            this.SettingsManager = SettingsManager;

            RestoreAndRemovePathsOfDeletedProjects().Wait();
        }

        public async Task RestoreAndRemovePathsOfDeletedProjects()
        {
            await RestoreSettings();

            foreach (var path in LastKnownProjectPaths.ToArray())
            {
                if (!IsPathAProject(path))
                    LastKnownProjectPaths.Remove(path);
            }
            await SaveSettings();
        }

        /// <summary>
        ///  Will run Path initialization, Refresh and Restore settings asynchronously.
        ///  Script compilation will happen async on purpose, so we can start app before it finishes since it is a long operation.
        /// </summary>
        public override Task InitProject(string path) => InitProjectInternal(path, true);
        public void InitProjectNoScriptCompile(string path) => InitProjectInternal(path, false);

        private Task InitProjectInternal(string path, bool compileScripts)
        {
            CreateProjectPaths(path);

            // No need to wait for this task. It is relevant to finish before next project startup, which is not that often and task is pretty quick
            var rememberPathTask = RememberPathInSettings(path);

            AssetManager.Refresh();
            SettingsManager.RestoreSettings();

            if (!compileScripts)
            {
                // Commented out on purpose.
                // Settings CanLoadAssets will make systems load assets even if compilation is not complete.
                // Which often can produce unneded results. Should not load anything if compilation is not complete.
                // AssetManager.CanLoadAssets = true;
                // AssetManager.Refresh();

                base.OnNewProjectOpened(path);
                return Task.CompletedTask;
            }
            else
            {
                return Task.Run(async () =>
                {
                    await ScriptManager.CompileScriptsAndReloadUserDomain();

                    AssetManager.CanLoadAssets = true;
                    AssetManager.Refresh();

                    base.OnNewProjectOpened(path);
                });
            }
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

        public async Task RestoreSettings()
        {
            var lastPaths = m_Serializer.LoadObject<string[]>(FilePath);

            if (lastPaths != null)
                LastKnownProjectPaths = lastPaths.ToList();

            if (LastKnownProjectPaths == null)
                LastKnownProjectPaths = new List<string>();

            await Task.CompletedTask;
        }

        public async Task SaveSettings()
        {
            if (LastKnownProjectPaths != null)
                m_Serializer.SaveObject(FilePath, LastKnownProjectPaths.ToArray());

            await Task.CompletedTask;
        }

        public async Task RememberPathInSettings(string path)
        {
            await RestoreSettings();

            if (LastKnownProjectPaths.Contains(path))
                LastKnownProjectPaths.Remove(path);

            LastKnownProjectPaths.Insert(0, path);
            await SaveSettings();
        }

        private static void CreateProjectPaths(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            Environment.CurrentDirectory = path;

            foreach (var projPath in Paths.ProjectPathArray)
            {
                if (!Directory.Exists(projPath))
                    Directory.CreateDirectory(projPath);
            }
        }
    }
}
