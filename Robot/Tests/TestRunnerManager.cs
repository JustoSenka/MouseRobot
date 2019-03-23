using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace Robot.Tests
{
    /// <summary>
    /// Contains list of all tests and test fixtures in project.
    /// </summary>
    public class TestRunnerManager : ITestRunnerManager
    {
        private IList<string> m_ModifiedFilesSinceLastUpdate = new List<string>();

        /// <summary>
        /// Returns a copy list of all existing Fixtures in project.
        /// </summary>
        public IList<TestFixture> TestFixtures { get { return m_TestFixtures.ToList(); } }
        private IList<TestFixture> m_TestFixtures;

        public event Action<TestFixture, int> TestFixtureAdded;
        public event Action<TestFixture, int> TestFixtureRemoved;
        public event Action<TestFixture, int> TestFixtureModified;

        private readonly IUnityContainer Container;
        private readonly ITestStatusManager TestStatusManager;
        private readonly IAssetManager AssetManager;
        private readonly IProfiler Profiler;
        public TestRunnerManager(IUnityContainer Container, ITestStatusManager TestStatusManager, IAssetManager AssetManager, IProfiler Profiler)
        {
            this.Container = Container;
            this.TestStatusManager = TestStatusManager;
            this.AssetManager = AssetManager;
            this.Profiler = Profiler;

            m_TestFixtures = new List<TestFixture>();

            AssetManager.AssetCreated += AddPathToList;
            AssetManager.AssetUpdated += AddPathToList;
            AssetManager.AssetDeleted += AddPathToList;
            AssetManager.AssetRenamed += AddPathToListForRenaming;
            AssetManager.RefreshFinished += OnAssetRefreshFinished;
        }

        #region AssetManager Callbacks

        private void AddPathToList(string assetPath)
        {
            if (assetPath.EndsWith(FileExtensions.TestD))
                m_ModifiedFilesSinceLastUpdate.Add(assetPath);

            // If asset manager is not set to batch asset editing mode, that means no refresh will be called,
            // but something has already changed from within app. Call refresh callback manually.
            if (!AssetManager.IsEditingAssets)
                OnAssetRefreshFinished();
        }

        private void AddPathToListForRenaming(string oldPath, string newPath)
        {
            AddPathToList(newPath);
        }

        private void OnAssetRefreshFinished()
        {
            var firstReload = m_TestFixtures.Count == 0;
            if (m_ModifiedFilesSinceLastUpdate.Count == 0 && !firstReload)
                return;

            ReloadTestFixtures(firstReload);
            TestStatusManager.UpdateTestStatusForNewFixtures(m_TestFixtures.Select(f => f.ToLightTestFixture()));
        }

        #endregion // AssetManager Callbacks

        private void ReloadTestFixtures(bool firstReload = false)
        {
            Profiler.Start("TestRunnerManager.ReloadTestFixtures");

            var fixtureAssets = AssetManager.Assets.Where(asset => asset.Importer.HoldsType() == typeof(LightTestFixture));

            // Update test fixtures with modified values
            foreach (var asset in fixtureAssets)
            {
                // Ignore non-modified assets
                if (!firstReload && !m_ModifiedFilesSinceLastUpdate.Contains(asset.Path))
                    continue;

                var lightFixture = asset.Importer.Load<LightTestFixture>();
                if (lightFixture == null)
                    continue;

                // Use file name instead of serialized one
                lightFixture.Name = asset.Name;

                // Making a deep clone so modifying fixtures in other windows will not affect the status on TestRunner
                lightFixture = (LightTestFixture)lightFixture.Clone();

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

            m_ModifiedFilesSinceLastUpdate.Clear();

            Profiler.Stop("TestRunnerManager.ReloadTestFixtures");
        }
    }
}
