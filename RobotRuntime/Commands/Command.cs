using RobotRuntime.Execution;
using System;

namespace RobotRuntime
{
    [Serializable]
    public abstract class Command : IRunnable, ICloneable, ISimilar
    {
        public abstract string Name { get; }
        public abstract bool CanBeNested { get; }

        public abstract void Run();
        public abstract object Clone();

        public void Run(IRunner runner)
        {
            runner.Run(this);
        }

        public bool Similar(object obj)
        {
            var c = obj as Command;
            if (c == null)
                return false;

            return this.ToString() == c.ToString();
        }
    }
}
