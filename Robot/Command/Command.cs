using System;

namespace Robot
{
    [Serializable]
    public class Command : ICloneable
    {
        public CommandCode Code { set; get; }
        public string Text { set; get; }
        public int[] Args { set; get; }

        [NonSerialized]
        private Action m_RunMethod;

        public Command(Action runMethod, string text, CommandCode code, params int[] args)
        {
            m_RunMethod = runMethod;
            Text = text;
            Code = code;
            Args = args;
        }

        public void Run()
        {
            m_RunMethod();
        }

        public void ClearMethod()
        {
            m_RunMethod = null;
        }

        public override string ToString()
        {
            return Text;
        }

        public object Clone()
        {
            return new Command(this.m_RunMethod, this.Text, this.Code, this.Args);
        }
    }
}
