using RobotRuntime;
using RobotRuntime.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Robot
{
    public class ScriptManager : IEnumerable<Script>
    {
        static private ScriptManager m_Instance = new ScriptManager();
        static public ScriptManager Instance { get { return m_Instance; } }

        private readonly IList<Script> m_LoadedScripts;
        public IReadOnlyList<Script> LoadedScripts { get { return m_LoadedScripts.ToList().AsReadOnly(); } }

        private Script m_ActiveScript;
        public Script ActiveScript {
            set
            {
                if (m_ActiveScript != value)
                    activeScriptChanged?.Invoke(m_ActiveScript, value);

                m_ActiveScript = value;
            }
            get { return m_ActiveScript; }
        }

        public event Action<Script, Script> activeScriptChanged;
        public event Action<Script> scriptLoaded;
        public event Action<int> scriptRemoved;
        public event Action<Script, Command> CommandAddedToScript;
        public event Action<Script, Command> CommandModifiedOnScript;

        private ScriptManager()
        {
            m_LoadedScripts = new List<Script>();
        }

        public void InvokeCommandAddedToScript(Script script, Command command)
        {
            CommandAddedToScript?.Invoke(script, command);
        }

        public void InvokeCommandModifiedOnScript(Script script, Command command)
        {
            CommandModifiedOnScript?.Invoke(script, command);
        }

        public Script NewScript(Script clone = null)
        {
            Script script;
            
            if (clone == null)
                script = new Script();
            else
                script = (Script) clone.Clone();

            m_LoadedScripts.Add(script);
            script.IsDirty = true; 

            MakeSureActiveScriptExist();
            return script;
        }

        public void RemoveScript(Script script)
        {
            var position = m_LoadedScripts.IndexOf(script);

            m_LoadedScripts.Remove(script);

            scriptRemoved?.Invoke(position);

            MakeSureActiveScriptExist();
        }

        public void RemoveScript(int position)
        {
            m_LoadedScripts.RemoveAt(position);
            scriptRemoved?.Invoke(position);

            MakeSureActiveScriptExist();
        }

        public Script LoadScript(string path)
        {
            if (m_LoadedScripts.FirstOrDefault(s => s.Path.Equals(path)) != null)
            {
                System.Diagnostics.Debug.WriteLine("Script is already loaded: " + path);
                return null;
            }

            if (m_LoadedScripts.Count == 1 && m_LoadedScripts[0].Name == Script.DefaultScriptName && m_LoadedScripts[0].Commands.Count == 0)
                RemoveScript(0);

            var script = AssetManager.Instance.GetAsset(path).Importer.Load<Script>();
            script.Path = path;

            System.Diagnostics.Debug.WriteLine("Script loaded: " + path);

            m_LoadedScripts.Add(script);
            scriptLoaded?.Invoke(script);

            MakeSureActiveScriptExist();
            return script;
        }

        public void SaveScript(Script script, string path)
        {
            AssetManager.Instance.CreateAsset(script, path);
            script.Path = Commons.GetProjectRelativePath(path);

            Console.WriteLine("Script saved: " + path);
        }

        public void MoveCommandAfter(int commandIndex, int positionAfter, int scriptIndex, int destinationScriptIndex = -1)
        {
            if (scriptIndex == destinationScriptIndex || destinationScriptIndex == -1) // Same script
            {
                m_LoadedScripts[scriptIndex].MoveCommandAfter(commandIndex, positionAfter);
            }
            else // Move between two different scripts
            {
                var command = m_LoadedScripts[scriptIndex].Commands[commandIndex];
                m_LoadedScripts[scriptIndex].RemoveCommand(commandIndex);
                m_LoadedScripts[destinationScriptIndex].InsertCommand(positionAfter + 1, command);

                m_LoadedScripts[destinationScriptIndex].IsDirty = true;
            }

            m_LoadedScripts[scriptIndex].IsDirty = true;
        }

        public void MoveScriptAfter(int index, int after)
        {
            m_LoadedScripts.MoveAfter(index, after);
        }

        public void MoveScriptBefore(int index, int before)
        {
            m_LoadedScripts.MoveBefore(index, before);
        }

        public Script GetScriptFromCommand(Command command)
        {
            return LoadedScripts.FirstOrDefault((s) => s.Commands.Contains(command));
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

        public IEnumerator<Script> GetEnumerator()
        {
            return m_LoadedScripts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_LoadedScripts.GetEnumerator();
        }
    }
}
