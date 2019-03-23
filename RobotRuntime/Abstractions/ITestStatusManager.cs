using System;
using System.Collections.Generic;
using RobotRuntime.Tests;

namespace RobotRuntime.Abstractions
{
    public interface ITestStatusManager
    {
        Dictionary<(string fix, string rec), TestStatus> CurrentStatus { get; }

        event Action TestStatusUpdated;
        string OutputFilePath { get; }

        IEnumerable<string> GetFormattedTestRunStatus();
        void OutputTestRunStatusToFile(string path = "");

        TestStatus GetFixtureStatus(LightTestFixture fixture);
        void ResetTestStatusForModifiedTests(LightTestFixture oldFixture, LightTestFixture newFixture);
        void UpdateTestStatusForNewFixtures(IEnumerable<LightTestFixture> fixtures);
    }
}