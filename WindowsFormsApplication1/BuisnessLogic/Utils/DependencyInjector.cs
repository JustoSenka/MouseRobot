using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows.Forms;
using MouseRobotUI;

namespace MouseRobot
{
    public class DependencyInjector
    {

        public static Form GetMainForm()
        {
            if (CheckMainForm("MainForm"))
            {
                return new MainForm();
            }
            else if (CheckMainForm("RecordingForm"))
            {
                return new RecordingForm();
            }
            else
            {
                throw new CorruptedConfigsException
                    ("Key \"MainForm\" is corrupted in App.config");
            }
        }

        [Obsolete("Better use lazy MouseRobot")]
        public static IMouseRobot GetMouseRobot()
        {
            return new MouseRobotImpl(GetScriptThread());
        }

        public static Lazy<IMouseRobot> GetLazyMouseRobot()
        {
            return new Lazy<IMouseRobot>(() => new MouseRobotImpl(GetScriptThread()));
        }

        public static IScriptThread GetScriptThread()
        {
            if (CheckScriptThread("ScriptThreadImpl"))
            {
                return new ScriptThreadImpl();
            }
            else if (CheckScriptThread("ScriptSecondThreadImpl"))
            {
                return null; // new ScriptThreadSecondImpl();
            }
            else
            {
                throw new CorruptedConfigsException
                    ("Key \"IScriptThread\" is corrupted in App.config");
            }
        }

        public static ICommand GetCommand(Action run, string text, CommandCode code, params int[] args)
        {
            return new Command(run, text, code, args);
        }



        private static bool CheckMainForm(string str)
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Contains("MainForm"))
            {
                throw new CorruptedConfigsException
                    ("Key \"MainForm\" does not exist in App.config");
            }
            return ConfigurationManager.AppSettings["MainForm"].Equals(str);
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
