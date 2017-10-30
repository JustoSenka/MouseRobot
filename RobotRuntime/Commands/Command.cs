using System;

namespace RobotRuntime
{
    [Serializable]
    public abstract class Command : ICloneable
    {
        public string Text { set; get; }

        public abstract void Run();
        public abstract object Clone();

        public override string ToString()
        {
            return Text;
        }
    }
}
