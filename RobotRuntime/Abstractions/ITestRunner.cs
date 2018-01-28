using System;
using RobotRuntime.Execution;

namespace RobotRuntime.Abstractions
{
    public interface ITestRunner
    {
        event Action Finished;
        event CommandRunningCallback RunningCommand;

        void Start(LightScript lightScript);
        void Stop();
    }
}