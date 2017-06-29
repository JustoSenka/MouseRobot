using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseRobot
{
    [Serializable]
    public class Command : ICommand
    {
        private Action RunMethod { set; get; }
        public CommandCode Code { set; get; }
        public string Text { set; get; }
        public int[] Args { set; get; }

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

        public void ClearMethod()
        {
            RunMethod = null;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
