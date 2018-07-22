using System;
using System.Collections.Generic;
using Robot.Tests;
using RobotRuntime.Tests;

namespace Robot.Abstractions
{
    public interface ITestRunnerManager
    {
        IList<TestFixture> TestFixtures { get; }
        Dictionary<Tuple<string, string>, TestStatus> TestStatusDictionary { get; }

        event Action<TestFixture, int> TestFixtureAdded;
        event Action<TestFixture, int> TestFixtureRemoved;
        event Action<TestFixture, int> TestFixtureModified;

        event Action TestStatusUpdated;

        TestStatus GetFixtureStatus(TestFixture fixture);
    }
}