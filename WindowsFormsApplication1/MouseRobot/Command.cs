using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseRobot
{
    public class Command
    {
        public delegate void Action();
        private Action runMethod;

        public string Text { private set; get; }

        public Command(Action runMethod, string text)
        {
            this.runMethod = runMethod;
            Text = text;
        }

        public void Run()
        {
            runMethod();
        }
    }
}
