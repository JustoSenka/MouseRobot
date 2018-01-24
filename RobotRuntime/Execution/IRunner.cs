using System;

namespace RobotRuntime.Execution
{
    public interface IRunner
    {
        void Run(IRunnable runnable);
        bool IsValidForType(Type type);
    }

    public delegate void CommandRunningCallback(Command command);
}
