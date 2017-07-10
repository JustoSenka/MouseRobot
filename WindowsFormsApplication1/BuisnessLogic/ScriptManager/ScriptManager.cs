using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseRobot
{
    public class ScriptManager : IScriptManager
    {
        public IList<Script> loadedScripts { get; }
        public Script activeScript { get; set; }

        public ScriptManager()
        {
            loadedScripts = new List<Script>();
        }

        public Script NewScript()
        {
            var script = new Script();
            loadedScripts.Add(script);

            MakeSureActiveScriptExist();
            return script;
        }

        public void RemoveScript(Script script)
        {
            loadedScripts.Remove(script);
            MakeSureActiveScriptExist();
        }

        public void RemoveScript(int position)
        {
            RemoveScript(loadedScripts[position]);
        }

        public Script LoadScript(string path)
        {
            foreach (var s in loadedScripts)
            {
                if (s.path.Equals(path))
                {
                    Console.WriteLine("ERROR: Script is already loaded: " + path);
                    return null;
                }
            }

            var script = (Script)BinaryObjectIO.LoadObject<Script>(path).Clone();
            script.path = path;

            Console.WriteLine("File loaded.");
            loadedScripts.Add(script);

            MakeSureActiveScriptExist();
            return script;
        }

        public void SaveScript(Script script, string path)
        {
            var scriptToSave = (Script)script.Clone();
            foreach (var v in scriptToSave.commands)
            {
                v.ClearMethod(); // Don't save actions/delegates, they're not serializable
            }
            BinaryObjectIO.SaveObject(path, scriptToSave);

            Console.WriteLine("File saved.");
        }

        private void MakeSureActiveScriptExist()
        {
            if (!loadedScripts.Contains(activeScript) || loadedScripts.Count == 0)
                activeScript = null;
            
            if (loadedScripts.Count == 1)
                activeScript = loadedScripts[0];

            if (activeScript == null && loadedScripts.Count > 0)
                activeScript = loadedScripts[0];
        }
    }
}
