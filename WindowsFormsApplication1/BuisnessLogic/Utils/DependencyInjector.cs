using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace MouseRobot
{
    public class DependencyInjector
    {

        [Obsolete("Better use lazy MouseRobot")]
        public static IMouseRobot getMouseRobot()
        {
            return new MouseRobotImpl(getScriptThread());
        }

        public static Lazy<IMouseRobot> getLazyMouseRobot()
        {
            return new Lazy<IMouseRobot>(() => new MouseRobotImpl(getScriptThread()));
        }

        public static IScriptThread getScriptThread()
        {
            if (CheckScriptThread("ScriptThreadImpl"))
            {
                return new ScriptThreadImpl();
            }
            else if (CheckScriptThread("ScriptSecondThreadImpl"))
            {
                return new ScriptThreadSecondImpl();
            }
            else
            {
                throw new CorruptedConfigsException
                    ("Key \"IScriptThread\" is corrupted in App.config");
            }
        }

        public static ICommand getCommand(Action run, string text, CommandCode code, params int[] args)
        {
            return new Command(run, text, code, args);
        }



        private static bool CheckScriptThread(string str)
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Contains("IScriptThread"))
            {
                throw new CorruptedConfigsException
                    ("Key \"IScriptThread\" does not exist in App.config");
            }
            return ConfigurationManager.AppSettings["IScriptThread"].Equals(str);
        }
    }
}
