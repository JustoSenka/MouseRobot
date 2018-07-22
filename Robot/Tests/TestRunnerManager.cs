using System;
using System.Collections.Generic;
using System.Linq;
using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Scripts;
using RobotRuntime.Tests;
using Unity;

namespace Robot.Tests
{
    /// <summary>
    /// Contains list of all tests and test fixtures in project
    /// TestRunnerWindow relies on this class callbacks
    /// </summary>
    public class TestRunnerManager : ITestRunnerManager
    {
        private IList<string> m_ModifiedFilesSinceLastUpdate = new List<string>();

        /// <summary>
        /// Returns a copy list of all estFixtures in project.
        /// </summary>
        public IList<TestFixture> TestFixtures { get { return m_TestFixtures.ToList(); } }
        private IList<TestFixture> m_TestFixtures;

        public Dictionary<Tuple<string, string>, TestStatus> TestStatusDictionary { get; } = new Dictionary<Tuple<string, string>, TestStatus>();

        public event Action<TestFixture, int> TestFixtureAdded;
        public event Action<TestFixture, int> TestFixtureRemoved;
        public event Action<TestFixture, int> TestFixtureModified;

        private IUnityContainer Container;
        private IAssetManager AssetManager;
        public TestRunnerManager(IUnityContainer Container, IAssetManager AssetManager, ITestRunner TestRunner)
        {
            this.Container = Container;
            this.AssetManager = AssetManager;

            m_TestFixtures = new List<TestFixture>();

            AssetManager.AssetCreated += AddPathToList;
            AssetManager.AssetUpdated += AddPathToList;
            AssetManager.AssetDeleted += AddPathToList;
            AssetManager.AssetRenamed += AddPathToListForRenaming;
            AssetManager.RefreshFinished += OnAssetRefreshFinished;

            TestRunner.FixtureSpecialScripFailed += OnTestFailed;
            TestRunner.FixtureSpecialScriptSucceded += OnTestPassed;
            TestRunner.TestFailed += OnTestFailed;
            TestRunner.TestPassed += OnTestPassed;
        }

        #region TestRunner Callbacks

        private void OnTestFailed(LightTestFixture fixture, Script script)
        {
            var tuple = CreateTuple(fixture, script);
            if (TestStatusDictionary.ContainsKey(tuple))
                TestStatusDictionary[tuple] = TestStatus.Failed;
        }

        private void OnTestPassed(LightTestFixture fixture, Script script)
        {
            var tuple = CreateTuple(fixture, script);
            if (TestStatusDictionary.ContainsKey(tuple))
                TestStatusDictionary[tuple] = TestStatus.Passed;
        }

        #endregion // TestRunner Callbacks

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
            UpdateTestStatus();
        }

        #endregion // AssetManager Callbacks

        private void ReloadTestFixtures(bool firstReload = false)
        {
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
                else
                {
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
        }

        private void UpdateTestStatus()
        {
            foreach (var fixture in m_TestFixtures)
            {
                foreach (var script in fixture.LoadedScripts)
                {
                    var tuple = CreateTuple(fixture, script);
                    if (!TestStatusDictionary.ContainsKey(tuple))
                        TestStatusDictionary.Add(tuple, TestStatus.None);
                }
            }
        }

        private Tuple<string, string> CreateTuple(TestFixture fixture, Script script)
        {
            return new Tuple<string, string>(fixture.Name, script.Name);
        }

        private Tuple<string, string> CreateTuple(LightTestFixture fixture, Script script)
        {
            return new Tuple<string, string>(fixture.Name, script.Name);
        }
    }

    public enum TestStatus
    {
        None, Passed, Failed
    }
}
