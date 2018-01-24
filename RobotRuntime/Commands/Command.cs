using RobotRuntime.Execution;
using System;

namespace RobotRuntime
{
    [Serializable]
    public abstract class Command : IRunnable, ICloneable
    {
        public abstract void Run();
        public abstract object Clone();

        public void Run(IRunner runner)
        {
            runner.Run(this);
        }

        public abstract CommandType CommandType { get; }
    }

    public enum CommandType
    {
        Down, Move, Press, Release, Sleep, ForImage, ForeachImage
    }
}
