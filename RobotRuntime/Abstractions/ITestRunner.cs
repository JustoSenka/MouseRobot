using System;
using RobotRuntime.Execution;

namespace RobotRuntime.Abstractions
{
    public interface ITestRunner
    {
        event Action Finished;
        event CommandRunningCallback RunningCommandCallback;

        void Start(string projectPath, string scriptName);
        void Start(LightScript lightScript);
        void Stop();
    }
}