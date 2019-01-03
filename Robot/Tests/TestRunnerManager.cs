using System;
using System.Collections.Generic;
using System.Linq;
using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using Unity;

namespace Robot.Tests
{
    /// <summary>
    /// Contains list of all tests and test fixtures in project.
    /// Also holds dictionary of TestStatus
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
        private readonly object m_TestStatusDictionaryLock = new object();

        public event Action<TestFixture, int> TestFixtureAdded;
        public event Action<TestFixture, int> TestFixtureRemoved;
        public event Action<TestFixture, int> TestFixtureModified;

        public event Action TestStatusUpdated;

        private IUnityContainer Container;
        private IAssetManager AssetManager;
        private IProfiler Profiler;
        public TestRunnerManager(IUnityContainer Container, IAssetManager AssetManager, ITestRunner TestRunner, IProfiler Profiler)
        {
            this.Container = Container;
            this.AssetManager = AssetManager;
            this.Profiler = Profiler;

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

        public TestStatus GetFixtureStatus(TestFixture fixture)
        {
            KeyValuePair<Tuple<string, string>, TestStatus>[] statusList;
            lock (m_TestStatusDictionaryLock)
            {
                statusList = TestStatusDictionary.ToArray();
            }

            var status = TestStatus.Passed;
            foreach (var pair in statusList)
            {
                if (pair.Key.Item1 != fixture.Name)
                    continue;

                if (pair.Value == TestStatus.Failed)
                {
                    status = TestStatus.Failed;
                    break;
                }

                if (pair.Value == TestStatus.None)
                    status = TestStatus.None;
            }

            return status;
        }

        private void SetStatus(Tuple<string, string> tuple, TestStatus status)
        {
            if (TestStatusDictionary.ContainsKey(tuple))
                TestStatusDictionary[tuple] = status;
            else
                TestStatusDictionary.Add(tuple, status);
        }

        #region TestRunner Callbacks

        private void OnTestFailed(LightTestFixture fixture, RobotRuntime.Recordings.Recording script)
        {
            lock (m_TestStatusDictionaryLock)
            {
                var tuple = CreateTuple(fixture, script);
                SetStatus(tuple, TestStatus.Failed);
                TestStatusUpdated?.Invoke();
            }
        }

        private void OnTestPassed(LightTestFixture fixture, RobotRuntime.Recordings.Recording script)
        {
            lock (m_TestStatusDictionaryLock)
            {
                var tuple = CreateTuple(fixture, script);
                SetStatus(tuple, TestStatus.Passed);
                TestStatusUpdated?.Invoke();
            }
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

                lightFixture.Name = asset.Name;

                // Making a deep clone so modifying fixtures in other windows will not affect the status on TestRunner
                lightFixture = (LightTestFixture) lightFixture.Clone();

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
                    ResetTestStatusForModifiedTests(fixture, lightFixture);
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

        /// <summary>
        /// This method constructs new dictionary from all tests in old fixture, then iterates new fixture to find matching scripts
        /// Iterating both fixtures would result in O(n^2). With dictionary, it is only O(2n) with some temp memory allocations.
        /// </summary>
        private void ResetTestStatusForModifiedTests(TestFixture oldFixture, LightTestFixture newFixture)
        {
            // dictionary with count is faster on adding elements. Operation complexity is o(1)
            var newScriptsDict = new Dictionary<string, RobotRuntime.Recordings.Recording>(oldFixture.Tests.Count);
            foreach (var s in oldFixture.Tests)
                newScriptsDict.Add(s.Name, s);

            var oldScripts = newFixture.Tests;

            for (int i = 0; i < oldScripts.Count; ++i)
            {
                var oldScript = oldScripts[i];
                RobotRuntime.Recordings.Recording newScript;
                newScriptsDict.TryGetValue(oldScript.Name, out newScript);

                // if new script is different, mark it's status as None
                if (!oldScript.Similar(newScript) && newScript != null)
                {
                    lock (m_TestStatusDictionaryLock)
                    {
                        var tuple = CreateTuple(oldFixture, newScript);
                        SetStatus(tuple, TestStatus.None);

                        TestStatusUpdated?.Invoke();
                    }
                }
            }
        }

        private void UpdateTestStatus()
        {
            lock (m_TestStatusDictionaryLock)
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
            TestStatusUpdated?.Invoke();
        }

        private Tuple<string, string> CreateTuple(TestFixture fixture, RobotRuntime.Recordings.Recording script)
        {
            return new Tuple<string, string>(fixture.Name, script.Name);
        }

        private Tuple<string, string> CreateTuple(LightTestFixture fixture, RobotRuntime.Recordings.Recording script)
        {
            return new Tuple<string, string>(fixture.Name, script.Name);
        }
    }

    public enum TestStatus
    {
        None, Passed, Failed
    }
}
