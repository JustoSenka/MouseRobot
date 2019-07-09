using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Scripts;
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
        public IList<TestFixture> TestFixtures { get { return m_TestFixtures.ToList(); } }
        private IList<TestFixture> m_TestFixtures;

        public event Action<TestFixture, int> TestFixtureAdded;
        public event Action<TestFixture, int> TestFixtureRemoved;
        public event Action<TestFixture, int> TestFixtureModified;

        private bool m_FirstScriptRecompilationHappened = false;

        private readonly IUnityContainer Container;
        private readonly ITestStatusManager TestStatusManager;
        private readonly IAssetManager AssetManager;
        private readonly IProfiler Profiler;
        private readonly TypeCollector<Command> TypeCollector;
        public TestRunnerManager(IUnityContainer Container, ITestStatusManager TestStatusManager, IAssetManager AssetManager, IProfiler Profiler, ITestRunner TestRunner,
            TypeCollector<Command> TypeCollector, IModifiedAssetCollector AssetCollector)
        {
            this.Container = Container;
            this.TestStatusManager = TestStatusManager;
            this.AssetManager = AssetManager;
            this.Profiler = Profiler;
            this.TypeCollector = TypeCollector;

            m_TestFixtures = new List<TestFixture>();

            AssetCollector.ExtensionFilters.Add(FileExtensions.TestD);

            AssetCollector.AssetsModified += OnAssetRefreshFinished;
            TypeCollector.NewTypesAppeared += OnNewTypesAppeared;

            TestRunner.TestRunEnd += OnTestsFinished;
        }

        private void OnTestsFinished()
        {
            TestStatusManager.OutputTestRunStatusToFile();
        }

        private void OnNewTypesAppeared()
        {
            if (!m_FirstScriptRecompilationHappened)
            {
                m_FirstScriptRecompilationHappened = true;

                ReloadTestFixtures(null, true);
                TestStatusManager.UpdateTestStatusForNewFixtures(m_TestFixtures.Select(f => f.ToLightTestFixture()));
            }
        }
        private void OnAssetRefreshFinished(IEnumerable<string> modifiedAssets)
        {
            var firstReload = m_TestFixtures.Count == 0;

            // Do not load if no assets were modified or if plugins have never been loaded (startup)
            if (modifiedAssets == null || !m_FirstScriptRecompilationHappened || (modifiedAssets.Count() == 0 && !firstReload))
                return;

            ReloadTestFixtures(modifiedAssets, firstReload);
            TestStatusManager.UpdateTestStatusForNewFixtures(m_TestFixtures.Select(f => f.ToLightTestFixture()));
        }

        private void ReloadTestFixtures(IEnumerable<string> modifiedAssets, bool firstReload = false)
        {
            Profiler.Start("TestRunnerManager.ReloadTestFixtures");

            var fixtureAssets = AssetManager.Assets.Where(asset => asset.Importer.HoldsType() == typeof(LightTestFixture));

            // Update test fixtures with modified values
            foreach (var asset in fixtureAssets)
            {
                if (asset.Importer.LoadingFailed)
                    continue;

                // Ignore non-modified assets
                if (!firstReload && !modifiedAssets.Contains(asset.Path))
                    continue;

                // Reloading asset on purpose, so it gives us up to date asset and
                //  gives a different refernce so unsaved modifications from other windows will not affect test run
                var lightFixture = asset.Importer.ReloadAsset<LightTestFixture>();
                if (lightFixture == null)
                    continue;

                // Use file name instead of serialized one
                lightFixture.Name = asset.Name;

                var fixture = m_TestFixtures.FirstOrDefault(f => f.Name == asset.Name);

                // Create new fixture if one does not exist
                if (fixture == null)
                {
                    fixture = Container.Resolve<TestFixture>();
                    fixture.ApplyLightFixtureValues(lightFixture);
                    fixture.Path = asset.Path;

                    m_TestFixtures.Add(fixture);
                    TestFixtureAdded?.Invoke(fixture, m_TestFixtures.Count - 1);
                }
                // Modify an existing one with new data from disk
                else
                {
                    TestStatusManager.ResetTestStatusForModifiedTests(fixture.ToLightTestFixture(), lightFixture);
                    fixture.ApplyLightFixtureValues(lightFixture);
                    fixture.Path = asset.Path;
                    TestFixtureModified?.Invoke(fixture, m_TestFixtures.IndexOf(fixture));
                }
            }

            var assetNames = fixtureAssets.Select(asset => asset.Name);

            // Remove deleted test fixtures
            for (int i = m_TestFixtures.Count - 1; i >= 0; --i)
            {
                if (!assetNames.Contains(m_TestFixtures[i].Name))
                {
                    var fixture = m_TestFixtures[i];
                    m_TestFixtures.RemoveAt(i);
                    TestFixtureRemoved?.Invoke(fixture, i);
                }
            }

            Profiler.Stop("TestRunnerManager.ReloadTestFixtures");
        }
    }
}
