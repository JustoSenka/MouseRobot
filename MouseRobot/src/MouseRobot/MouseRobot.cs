using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Robot
{
    public class MouseRobot
    {
        static private MouseRobot m_Instance = new MouseRobot();
        static public MouseRobot Instance { get { return m_Instance; } }

        private MouseRobot()
        {
            ScriptManager.Instance.NewScript();
        }

        public void StartScript(int repeatTimes)
        {
            if (ScriptManager.Instance.activeScript != null)

                if (ScriptManager.Instance.activeScript.Commands.Count <= 0)
                {
                    throw new EmptyScriptException("Script is empty");
                }
            ScriptThread.Instance.Start(ScriptManager.Instance.activeScript, repeatTimes);
        }

        public void StopScript()
        {
            // TO BE IMPLEMENTED
        }

        public void AddCommandSleep(int time)
        {
            ScriptManager.Instance.activeScript.AddCommandSleep(time);
        }

        public void AddCommandRelease()
        {
            ScriptManager.Instance.activeScript.AddCommandRelease();
        }

        public void AddCommandPress(int x, int y)
        {
            ScriptManager.Instance.activeScript.AddCommandPress(x, y);
        }

        public void AddCommandMove(int x, int y)
        {
            ScriptManager.Instance.activeScript.AddCommandMove(x, y);
        }

        public void AddCommandDown(int x, int y)
        {
            ScriptManager.Instance.activeScript.AddCommandDown(x, y);
        }

        public void EmptyScript()
        {
            ScriptManager.Instance.activeScript.EmptyScript();
        }

        public void NewScript()
        {
            ScriptManager.Instance.NewScript();
        }

        public void OpenScript(string path)
        {
            ScriptManager.Instance.LoadScript(path);
        }

        public void SaveScript(string path)
        {
            ScriptManager.Instance.SaveScript(ScriptManager.Instance.activeScript, path);
        }

        /*public IEnumerable<IEnumerable<string>> GetScriptTreeStructure()
        {
            foreach (var s in ScriptManager.Instance.loadedScripts)
                yield return s.CommandText;
        }*/

        

        public void RemoveScript(int index)
        {
            throw new NotImplementedException();
        }
    }
}
