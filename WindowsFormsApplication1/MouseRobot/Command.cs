using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseRobot
{
    [Serializable]
    public class Command
    {
        [Serializable]
        public delegate void Action();

        public Action RunMethod { set; get; }
        public CommandCode Code { private set; get; }
        public string Text { private set; get; }
        public int[] Args { private set; get; }


        public Command(Action runMethod, string text, CommandCode code, params int[] args)
        {
            RunMethod = runMethod;
            Text = text;
            Code = code;
            Args = args;
        }

        public void Run()
        {
            RunMethod();
        }
    }
}
