using System;
using RobotRuntime.Execution;

namespace RobotRuntime.Abstractions
{
    public interface ITestRunner
    {
        event Action TestRunEnd;
        event CommandRunningCallback RunningCommandCallback;

        void StartScript(string projectPath, string scriptName);
        void StartTests(string projectPath, string testFilter = "");

        void StartScript(LightScript lightScript);
        void StartTests(string testFilter = "");

        void Stop();
    }
}