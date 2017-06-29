using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseRobot
{
    public interface IMouseRobot
    {
        void StartScript(int reapeatTimes);
        void StopScript();

        void AddCommandSleep(int time);
        void AddCommandRelease();
        void AddCommandPress(int x, int y);
        void AddCommandMove(int x, int y);
        void AddCommandDown(int x, int y);
        void EmptyScript();

        void NewScript();
        void OpenScript(string fileName);
        void SaveScript(string fileName);

        TreeNode<string> GetScriptTreeStructure(); 
    }
}
