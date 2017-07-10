using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MouseRobotUI.BuisnessLogic;

namespace MouseRobot
{
    public partial class MouseRobotImpl : IMouseRobot
    {
        // public delegate void MyEventHandler(object sender, CustomEventArgs e);

        public IScriptThread scriptThread;
        public IScriptManager scriptManager;

        public MouseRobotImpl(IScriptThread scriptThread, IScriptManager scriptManager)
        {
            this.scriptThread = scriptThread;
            this.scriptManager = scriptManager;
            scriptManager.NewScript();
        }

        public void StartScript(int repeatTimes)
        {
            if (scriptManager.activeScript != null)

                if (scriptManager.activeScript.commands.Count <= 0)
                {
                    throw new EmptyScriptException("Script is empty");
                }
            scriptThread.Start(scriptManager.activeScript, repeatTimes);
        }

        public void StopScript()
        {
            if (scriptManager.activeScript.commands.Count != 0)
            {
                scriptThread.BreakEvent += scriptThread.OnBreakEvent;
            }
        }

        public void AddCommandSleep(int time)
        {
            scriptManager.activeScript.AddCommandSleep(time);
        }

        public void AddCommandRelease()
        {
            scriptManager.activeScript.AddCommandRelease();
        }

        public void AddCommandPress(int x, int y)
        {
            scriptManager.activeScript.AddCommandPress(x, y);
        }

        public void AddCommandMove(int x, int y)
        {
            scriptManager.activeScript.AddCommandMove(x, y);
        }

        public void AddCommandDown(int x, int y)
        {
            scriptManager.activeScript.AddCommandDown(x, y);
        }

        public void EmptyScript()
        {
            scriptManager.activeScript.EmptyScript();
        }

        public void NewScript()
        {
            scriptManager.NewScript();
        }

        public void OpenScript(string path)
        {
            scriptManager.LoadScript(path);
        }

        public void SaveScript(string path)
        {
            scriptManager.SaveScript(scriptManager.activeScript, path);
        }

        /*public IEnumerable<IEnumerable<string>> GetScriptTreeStructure()
        {
            foreach (var s in scriptManager.loadedScripts)
                yield return s.CommandText;
        }*/

        public TreeNode<string> GetScriptTreeStructure()
        {
            var tree = new TreeNode<string>("");
            foreach (var s in scriptManager.loadedScripts)
            {
                var child = tree.AddChild(s.Name);
                foreach (var c in s.commands)
                {
                    child.AddChild(c.Text);
                }
            }
            return tree;
        }
    }
}
