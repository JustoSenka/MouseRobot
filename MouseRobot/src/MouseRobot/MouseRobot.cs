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
            if (ScriptManager.Instance.ActiveScript != null)

                if (ScriptManager.Instance.ActiveScript.Commands.Count <= 0)
                {
                    throw new EmptyScriptException("Script is empty");
                }
            ScriptThread.Instance.Start(ScriptManager.Instance.ActiveScript, repeatTimes);
        }

        public void StopScript()
        {
            // TO BE IMPLEMENTED
        }

        public void AddCommandSleep(int time)
        {
            ScriptManager.Instance.ActiveScript.AddCommandSleep(time);
        }

        public void AddCommandRelease()
        {
            ScriptManager.Instance.ActiveScript.AddCommandRelease();
        }

        public void AddCommandPress(int x, int y)
        {
            ScriptManager.Instance.ActiveScript.AddCommandPress(x, y);
        }

        public void AddCommandMove(int x, int y)
        {
            ScriptManager.Instance.ActiveScript.AddCommandMove(x, y);
        }

        public void AddCommandDown(int x, int y)
        {
            ScriptManager.Instance.ActiveScript.AddCommandDown(x, y);
        }
    }
}
