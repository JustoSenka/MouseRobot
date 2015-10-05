using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseRobot
{
    public static class DependencyInjector
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
            return new ScriptThreadImpl();
            //return new ScriptThreadSecondImpl();
        }

        public static ICommand getCommand(Action run, string text, CommandCode code, params int[] args)
        {
            return new Command(run, text, code, args);
        }
    }
}
