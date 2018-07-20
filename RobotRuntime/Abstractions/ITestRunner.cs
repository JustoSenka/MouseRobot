using System;
using RobotRuntime.Tests;

namespace RobotRuntime.Abstractions
{
    public interface ITestRunner
    {
        TestData TestData { get; }
        event Action TestRunEnd;

        void StartScript(string projectPath, string scriptName);
        void StartTests(string projectPath, string testFilter = ".");

        void StartScript(LightScript lightScript);
        void StartTests(string testFilter = ".");

        void Stop();
    }
}