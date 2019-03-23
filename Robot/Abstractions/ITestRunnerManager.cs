using RobotRuntime.Tests;
using System;
using System.Collections.Generic;

namespace Robot.Abstractions
{
    public interface ITestRunnerManager
    {
        IList<TestFixture> TestFixtures { get; }

        event Action<TestFixture, int> TestFixtureAdded;
        event Action<TestFixture, int> TestFixtureRemoved;
        event Action<TestFixture, int> TestFixtureModified;
    }
}