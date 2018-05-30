using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using System;

namespace RobotRuntime.Execution
{
    public interface IRunner
    {
        void PassDependencies(IRunnerFactory RunnerFactory, LightScript TestFixture, 
            CommandRunningCallback Callback, ValueWrapper<bool> ShouldCancelRun);

        void Run(IRunnable runnable);
    }

    public delegate void CommandRunningCallback(Command command);
}
