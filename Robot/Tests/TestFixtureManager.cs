using Robot.Abstractions;
using RobotRuntime.Abstractions;
using RobotRuntime.Tests;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Robot.Tests
{
    public class TestFixtureManager : ITestFixtureManager
    {
        public IList<TestFixture> Fixtures { get; private set; }

        public event Action<TestFixture> FixtureAdded;
        public event Action<TestFixture> FixtureRemoved;
        public event Action<TestFixture, string> FixtureSaved;

        private IAssetManager AssetManager;
        private ICommandFactory CommandFactory;
        private IProfiler Profiler;
        private ILogger Logger;
        public TestFixtureManager(IAssetManager AssetManager, ICommandFactory CommandFactory, IProfiler Profiler, ILogger Logger)
        {
            this.AssetManager = AssetManager;
            this.CommandFactory = CommandFactory;
            this.Profiler = Profiler;
            this.Logger = Logger;

            Fixtures = new List<TestFixture>();
        }

        public TestFixture NewTestFixture()
        {
            var fixture = new TestFixture(AssetManager, CommandFactory, Profiler, Logger);
            Add(fixture);
            return fixture;
        }

        public TestFixture NewTestFixture(LightTestFixture lightTestFixture)
        {
            var fixture = new TestFixture(AssetManager, CommandFactory, Profiler, Logger);
            fixture.ApplyLightScriptValues(lightTestFixture);
            Add(fixture);
            return fixture;
        }

        public void Add(TestFixture fixture)
        {
            Fixtures.Add(fixture);
            FixtureAdded?.Invoke(fixture);
        }

        public void Remove(TestFixture fixture)
        {
            Fixtures.Remove(fixture);
            FixtureRemoved?.Invoke(fixture);
        }

        public void SaveTestFixture(TestFixture fixture, string path)
        {
            Profiler.Start("TestFixtureManager_SaveFixture");

            fixture.Name = Paths.GetName(path);
            fixture.Path = Paths.GetProjectRelativePath(path);
            AssetManager.CreateAsset(fixture.ToLightTestFixture(), path);
            fixture.IsDirty = false; // This will fire callback which will update UI dirty flag if needed.

            FixtureSaved?.Invoke(fixture, path);

            Profiler.Stop("TestFixtureManager_SaveFixture");
        }

        public bool Contains(string fixtureName)
        {
            return Fixtures.Any(f => f.Name == fixtureName);
        }
    }
}
