using Robot.Abstractions;
using Robot.Recordings;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using RobotRuntime.Utils;
using System;
using System.Linq;

namespace Robot
{
    public class ScriptManager : BaseHierarchyManager, IHierarchyManager
    {
        public static string k_DefaultScriptName = "New Script";

        private RobotRuntime.Recordings.Recording m_ActiveScript;
        public Recording ActiveScript
        {
            set
            {
                if (m_ActiveScript != value)
                    ActiveScriptChanged?.Invoke(m_ActiveScript, value);

                m_ActiveScript = value;
            }
            get { return m_ActiveScript; }
        }

        public event Action<RobotRuntime.Recordings.Recording, RobotRuntime.Recordings.Recording> ActiveScriptChanged;
        public event Action<RobotRuntime.Recordings.Recording> ScriptSaved;

        private IAssetManager AssetManager;
        private IProfiler Profiler;
        public ScriptManager(IAssetManager AssetManager, ICommandFactory CommandFactory, IProfiler Profiler, ILogger Logger)
            : base(CommandFactory, Profiler, Logger)
        {
            this.AssetManager = AssetManager;
            this.Profiler = Profiler;
        }

        public Recording LoadScript(string path)
        {
            var asset = AssetManager.GetAsset(path);
            if (asset == null)
            {
                Logger.Log(LogType.Error, "Cannot load script. No such asset at path: " + path);
                return null;
            }

            Profiler.Start("ScriptManager_LoadScript");

            // if hierarchy contains empty untitled script, remove it
            if (m_LoadedScripts.Count == 1 && m_LoadedScripts[0].Name == RobotRuntime.Recordings.Recording.DefaultScriptName && m_LoadedScripts[0].Commands.Count() == 0)
                RemoveScript(0);

            RobotRuntime.Recordings.Recording newScript = asset.Importer.ReloadAsset<RobotRuntime.Recordings.Recording>();
            if (newScript != null)
            {
                newScript.Path = asset.Path;
                AddScript(newScript, true);
            }
            else
                Logger.Log(LogType.Error, "Failed to load script: " + asset.Path);

            Profiler.Stop("ScriptManager_LoadScript");
            return newScript;
        }

        public void SaveScript(RobotRuntime.Recordings.Recording script, string path)
        {
            Profiler.Start("ScriptManager_SafeScript");

            AssetManager.CreateAsset(script, path);
            script.Path = Paths.GetProjectRelativePath(path);

            ScriptSaved?.Invoke(script);

            Profiler.Stop("ScriptManager_SafeScript");
        }

        private void MakeSureActiveScriptExist()
        {
            if (!m_LoadedScripts.Contains(ActiveScript) || m_LoadedScripts.Count == 0)
                ActiveScript = null;

            if (m_LoadedScripts.Count == 1)
                ActiveScript = m_LoadedScripts[0];

            if (ActiveScript == null && m_LoadedScripts.Count > 0)
                ActiveScript = m_LoadedScripts[0];
        }

        public override Recording NewScript(RobotRuntime.Recordings.Recording clone = null)
        {
            var s = base.NewScript(clone);
            s.Name = k_DefaultScriptName;
            MakeSureActiveScriptExist();
            return s;
        }

        public override void RemoveScript(RobotRuntime.Recordings.Recording script)
        {
            base.RemoveScript(script);
            MakeSureActiveScriptExist();
        }

        public override void RemoveScript(int position)
        {
            base.RemoveScript(position);
            MakeSureActiveScriptExist();
        }

        public override Recording AddScript(RobotRuntime.Recordings.Recording script, bool removeScriptWithSamePath = false)
        {
            var s = base.AddScript(script, removeScriptWithSamePath);
            MakeSureActiveScriptExist();
            return s;
        }
    }
}
