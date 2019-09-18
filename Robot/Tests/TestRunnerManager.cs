using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Reflection;
using RobotRuntime.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity;
using Unity.Lifetime;

namespace Robot.Tests
{
    /// <summary>
    /// Contains list of all tests and test fixtures in project.
    /// </summary>
    [RegisterTypeToContainer(typeof(ITestRunnerManager), typeof(ContainerControlledLifetimeManager))]
    public class TestRunnerManager : ITestRunnerManager
    {
        /// <summary>
        /// Returns a copy list of all existing Fixtures in project.
        /// </summary>
        public IList<TestFixture> TestFixtures { get { return m_TestFixtures.Values.ToList(); } }
        private Dictionary<Guid, TestFixture> m_TestFixtures;

        public event Action<TestFixture, int> TestFixtureAdded;
        public event Action<TestFixture, int> TestFixtureRemoved;
        public event Action<TestFixture, int> TestFixtureModified;

        private readonly object ReloadFixturesLock = new object();

        private readonly IUnityContainer Container;
        private readonly ITestStatusManager TestStatusManager;
        private readonly IAssetManager AssetManager;
        private readonly IProfiler Profiler;
        public TestRunnerManager(IUnityContainer Container, ITestStatusManager TestStatusManager, IAssetManager AssetManager, IProfiler Profiler,
            ITestRunner TestRunner, IModifiedAssetCollector AssetCollector, IProjectManager ProjectManager)
        {
            this.Container = Container;
            this.TestStatusManager = TestStatusManager;
            this.AssetManager = AssetManager;
            this.Profiler = Profiler;

            m_TestFixtures = new Dictionary<Guid, TestFixture>();

            AssetCollector.ExtensionFilters.Add(FileExtensions.TestD);

            AssetCollector.AssetsModified += OnAssetsModified;
            AssetCollector.AssetsRenamed += OnAssetsRenamed;

            ProjectManager.NewProjectOpened += _ => ReloadTestFixtures();

            TestRunner.TestRunEnd += OnTestsFinished;
        }

        public void ReloadTestFixtures()
        {
            ReloadTestFixtures(null, true);
            TestStatusManager.UpdateTestStatusForNewFixtures(m_TestFixtures.Select(f => f.Value.ToLightTestFixture()));
        }

        private void OnTestsFinished()
        {
            TestStatusManager.OutputTestRunStatusToFile();
        }

        private void OnAssetsModified(IEnumerable<string> modifiedAssets)
        {
            var firstReload = m_TestFixtures.Count == 0;

            // Do not reload if no assets were modified or if plugins have never been loaded (startup)
            if (modifiedAssets == null || !AssetManager.CanLoadAssets || (modifiedAssets.Count() == 0 && !firstReload))
                return;

            ReloadTestFixtures(modifiedAssets, firstReload);
            TestStatusManager.UpdateTestStatusForNewFixtures(m_TestFixtures.Select(f => f.Value.ToLightTestFixture()));
        }

        private void ReloadTestFixtures(IEnumerable<string> modifiedAssets, bool firstReload = false)
        {
            lock (ReloadFixturesLock)
            {
                Profiler.Begin("TestRunnerManager_ReloadTestFixtures", () =>
                {
                    if (!AssetManager.CanLoadAssets)
                        return;

                    var fixtureAssets = AssetManager.Assets.Where(asset => asset.HoldsType() == typeof(LightTestFixture));

                    // Update test fixtures with modified values
                    foreach (var asset in fixtureAssets)
                    {
                        if (asset.LoadingFailed)
                            continue;

                        // Ignore non-modified assets
                        if (!firstReload && !modifiedAssets.Contains(asset.Path))
                            continue;

                        // Reloading asset on purpose, so it gives us up to date asset and
                        //  gives a different refernce so unsaved modifications from other windows will not affect test run
                        var lightFixture = asset.ReloadAsset<LightTestFixture>();
                        if (lightFixture == null)
                            continue;

                        // Use file name instead of serialized one
                        lightFixture.Name = asset.Name;

                        var fixtureExists = m_TestFixtures.TryGetValue(lightFixture.Guid, out TestFixture fixture);

                        // Create new fixture if one does not exist
                        if (!fixtureExists)
                        {
                            fixture = Container.Resolve<TestFixture>();
                            fixture.ApplyLightFixtureValues(lightFixture);
                            fixture.Path = asset.Path;

                            m_TestFixtures[lightFixture.Guid] = fixture;
                            TestFixtureAdded?.Invoke(fixture, m_TestFixtures.Count - 1);
                        }
                        // Modify an existing one with new data from disk
                        else
                        {
                            TestStatusManager.ResetTestStatusForModifiedTests(fixture.ToLightTestFixture(), lightFixture);
                            fixture.ApplyLightFixtureValues(lightFixture);
                            fixture.Path = asset.Path;
                            TestFixtureModified?.Invoke(fixture, m_TestFixtures.Values.IndexOf(fixture));
                        }
                    }

                    var fixtureInAssetsGuids = fixtureAssets.Select(asset => asset.Load<LightTestFixture>().Guid);

                    // Remove deleted test fixtures
                    foreach (var testRunnerFixtureGuid in m_TestFixtures.Keys.ToArray())
                    {
                        // if fixture is not in assets folder anymore, remove it from TestRunnerManager
                        if (!fixtureInAssetsGuids.Contains(testRunnerFixtureGuid))
                        {
                            var fixture = m_TestFixtures[testRunnerFixtureGuid];
                            var index = m_TestFixtures.Values.IndexOf(fixture);

                            m_TestFixtures.Remove(testRunnerFixtureGuid);
                            TestFixtureRemoved?.Invoke(fixture, index);
                        }
                    }
                });
            }
        }

        private void OnAssetsRenamed(IEnumerable<(string From, string To)> renamedAssets)
        {
            if (renamedAssets == null || renamedAssets.Count() == 0)
                return;

            foreach ((var From, var To) in renamedAssets)
            {
                var asset = AssetManager.GetAsset(To);
                if (asset.LoadingFailed)
                    continue;

                var lightFixture = asset.Load<LightTestFixture>();
                var fixtureExists = m_TestFixtures.TryGetValue(lightFixture.Guid, out TestFixture fixture);

                if (!fixtureExists)
                {
                    Logger.Log(LogType.Error, "Fixture which was renamed does not exist in the TestRunnerManager: " + asset.Path);
                    return;
                }

                fixture.Path = asset.Path;
                fixture.Name = asset.Name;
                TestFixtureModified?.Invoke(fixture, m_TestFixtures.Values.IndexOf(fixture));
            }
        }
    }
}
