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

        public Script activeScript { get; set; }

        private ScriptManager()
        {
            m_LoadedScripts = new List<Script>();
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
            m_LoadedScripts.Remove(script);
            MakeSureActiveScriptExist();
        }

        public void RemoveScript(int position)
        {
            m_LoadedScripts.RemoveAt(position);
            MakeSureActiveScriptExist();
        }

        public Script LoadScript(string path)
        {
            foreach (var s in m_LoadedScripts)
            {
                if (s.Path.Equals(path))
                {
                    Console.WriteLine("ERROR: Script is already loaded: " + path);
                    return null;
                }
            }

            var script = (Script)BinaryObjectIO.LoadObject<Script>(path).Clone();
            script.Path = path;

            Console.WriteLine("File loaded.");
            m_LoadedScripts.Add(script);

            MakeSureActiveScriptExist();
            return script;
        }

        public void SaveScript(Script script, string path)
        {
            var scriptToSave = (Script)script.Clone();

            BinaryObjectIO.SaveObject(path, scriptToSave);
            script.Path = path;

            Console.WriteLine("File saved: " + path);
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


        private void MakeSureActiveScriptExist()
        {
            if (!m_LoadedScripts.Contains(activeScript) || m_LoadedScripts.Count == 0)
                activeScript = null;

            if (m_LoadedScripts.Count == 1)
                activeScript = m_LoadedScripts[0];

            if (activeScript == null && m_LoadedScripts.Count > 0)
                activeScript = m_LoadedScripts[0];
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
