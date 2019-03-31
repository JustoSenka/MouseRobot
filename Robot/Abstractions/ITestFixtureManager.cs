using System;
using System.Collections.Generic;
using Robot.Tests;
using RobotRuntime.Tests;

namespace Robot.Abstractions
{
    public interface ITestFixtureManager
    {
        IList<TestFixture> Fixtures { get; }

        event Action<TestFixture> FixtureAdded;
        event Action<TestFixture> FixtureRemoved;
        event Action<TestFixture, string> FixtureSaved;

        void Add(TestFixture fixture);
        void Remove(TestFixture fixture);

        TestFixture NewTestFixture();
        TestFixture NewTestFixture(LightTestFixture lightTestFixture);

        void SaveTestFixture(TestFixture fixture, string path);

        bool Contains(string fixtureName);
    }
}