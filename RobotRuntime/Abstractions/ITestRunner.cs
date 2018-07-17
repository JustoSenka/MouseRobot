using System;
using RobotRuntime.Execution;

namespace RobotRuntime.Abstractions
{
    public interface ITestRunner
    {
        event Action Finished;
        event CommandRunningCallback RunningCommandCallback;

        void StartScript(string projectPath, string scriptName);
        void StartScript(LightScript lightScript);
        void Stop();
    }
}