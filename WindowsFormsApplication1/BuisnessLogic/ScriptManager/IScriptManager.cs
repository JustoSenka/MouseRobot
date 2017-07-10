using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseRobot
{
    public interface IScriptManager
    {
        IList<Script> loadedScripts { get; }
        Script activeScript { get; set; }

        Script NewScript();

        void RemoveScript(Script script);
        void RemoveScript(int position);

        Script LoadScript(string path);
        void SaveScript(Script script, string path);
    }
}
