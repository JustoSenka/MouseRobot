﻿using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Tests;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Lifetime;

namespace Robot.Tests
{
    /// <summary>
    /// Contains TextFixtures which are currently open and loaded (has window document open).
    /// Responsible for keeping setup and teardown special recordings
    /// Handles saving of test fixtures
    /// Double clicking fixture in assets window will add fixture here
    /// TestFixtureWindow will rely on this manager callbacks
    /// </summary>
    [RegisterTypeToContainer(typeof(ITestFixtureManager), typeof(ContainerControlledLifetimeManager))]
    public class TestFixtureManager : ITestFixtureManager
    {
        public IList<TestFixture> Fixtures { get; private set; }

        public event Action<TestFixture> FixtureAdded;
        public event Action<TestFixture> FixtureRemoved;
        public event Action<TestFixture, string> FixtureSaved;

        private readonly IAssetManager AssetManager;
        private readonly ICommandFactory CommandFactory;
        private readonly IProfiler Profiler;
        private readonly ILogger Logger;
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
            fixture.ApplyLightFixtureValues(lightTestFixture);
            Add(fixture);
            return fixture;
        }

        public void Add(TestFixture fixture)
        {
            if (!Fixtures.Contains(fixture))
            {
                Fixtures.Add(fixture);
                FixtureAdded?.Invoke(fixture);
            }
            else
                Logger.Logi(LogType.Warning, "Cannot open test fixture because it is already open: " + fixture.Name);
        }

        public void Remove(TestFixture fixture)
        {
            if (Fixtures.Contains(fixture))
            {
                Fixtures.Remove(fixture);
                FixtureRemoved?.Invoke(fixture);
            }
        }

        public void SaveTestFixture(TestFixture fixture, string path)
        {
            Profiler.Start("TestFixtureManager_SaveFixture");

            fixture.Name = Paths.GetName(path);
            fixture.Path = Paths.GetRelativePath(path);
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
