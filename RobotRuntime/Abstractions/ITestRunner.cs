using System;
using RobotRuntime.Scripts;
using RobotRuntime.Tests;

namespace RobotRuntime.Abstractions
{
    public interface ITestRunner
    {
        TestData TestData { get; }

        event Action TestRunStart;
        event Action TestRunEnd;

        event Action<LightTestFixture> FixtureIsBeingRun;
        event Action<LightTestFixture, Script> TestIsBeingRun;

        event Action<LightTestFixture, Script> FixtureSpecialScripFailed;
        event Action<LightTestFixture, Script> FixtureSpecialScriptSucceded;
        event Action<LightTestFixture, Script> TestPassed;
        event Action<LightTestFixture, Script> TestFailed;

        void StartScript(string projectPath, string scriptName);
        void StartTests(string projectPath, string testFilter = ".");

        void StartScript(LightScript lightScript);
        void StartTests(string testFilter = ".");

        void Stop();
    }
}