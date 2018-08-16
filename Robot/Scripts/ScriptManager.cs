using Robot.Abstractions;
using Robot.Scripts;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Scripts;
using RobotRuntime.Utils;
using System;
using System.Linq;

namespace Robot
{
    public class ScriptManager : BaseScriptManager, IScriptManager
    {
        public static string k_DefaultScriptName = "New Script";

        private Script m_ActiveScript;
        public Script ActiveScript
        {
            set
            {
                if (m_ActiveScript != value)
                    ActiveScriptChanged?.Invoke(m_ActiveScript, value);

                m_ActiveScript = value;
            }
            get { return m_ActiveScript; }
        }

        public event Action<Script, Script> ActiveScriptChanged;
        public event Action<Script> ScriptSaved;

        private IAssetManager AssetManager;
        private IProfiler Profiler;
        public ScriptManager(IAssetManager AssetManager, ICommandFactory CommandFactory, IProfiler Profiler, ILogger Logger)
            : base(CommandFactory, Profiler, Logger)
        {
            this.AssetManager = AssetManager;
            this.Profiler = Profiler;
        }

        public Script LoadScript(string path)
        {
            var asset = AssetManager.GetAsset(path);
            if (asset == null)
            {
                Logger.Log(LogType.Error, "Cannot load script. No such asset at path: " + path);
                return null;
            }

            Profiler.Start("ScriptManager_LoadScript");

            // if hierarchy contains empty untitled script, remove it
            if (m_LoadedScripts.Count == 1 && m_LoadedScripts[0].Name == Script.DefaultScriptName && m_LoadedScripts[0].Commands.Count() == 0)
                RemoveScript(0);

            Script newScript = asset.Importer.ReloadAsset<Script>();
            newScript.Path = asset.Path;

            AddScript(newScript, true);

            Profiler.Stop("ScriptManager_LoadScript");
            return newScript;
        }

        public void SaveScript(Script script, string path)
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

        public override Script NewScript(Script clone = null)
        {
            var s = base.NewScript(clone);
            s.Name = k_DefaultScriptName;
            MakeSureActiveScriptExist();
            return s;
        }

        public override void RemoveScript(Script script)
        {
            base.RemoveScript(script);
            MakeSureActiveScriptExist();
        }

        public override void RemoveScript(int position)
        {
            base.RemoveScript(position);
            MakeSureActiveScriptExist();
        }

        public override Script AddScript(Script script, bool removeScriptWithSamePath = false)
        {
            var s = base.AddScript(script, removeScriptWithSamePath);
            MakeSureActiveScriptExist();
            return s;
        }
    }
}
