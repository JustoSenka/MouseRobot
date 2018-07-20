using RobotRuntime.Abstractions;
using RobotRuntime.Tests;

namespace RobotRuntime.Execution
{
    public interface IRunner
    {
        TestData TestData { set; get; }
        void Run(IRunnable runnable);
    }

    public delegate void CommandRunningCallback(Command command);
}
