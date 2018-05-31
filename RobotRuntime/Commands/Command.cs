using RobotRuntime.Execution;
using System;

namespace RobotRuntime
{
    [Serializable]
    public abstract class Command : IRunnable, ICloneable
    {
        public abstract string Name { get; }
        public abstract bool CanBeNested { get; }

        public abstract void Run();
        public abstract object Clone();

        public void Run(IRunner runner)
        {
            runner.Run(this);
        }
    }
}
