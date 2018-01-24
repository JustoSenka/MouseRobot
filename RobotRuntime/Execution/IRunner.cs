using System;

namespace RobotRuntime.Execution
{
    public interface IRunner
    {
        void Run(IRunnable runnable);
    }

    public delegate void CommandRunningCallback(Command command);
}
