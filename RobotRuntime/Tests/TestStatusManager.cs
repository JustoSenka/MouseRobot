using Newtonsoft.Json;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Lifetime;

namespace RobotRuntime.Tests
{
    /// <summary>
    /// Holds dictionary of TestStatus
    /// TestRunnerWindow and TestRunnerManager relies on this class callbacks
    /// </summary>
    [RegisterTypeToContainer(typeof(ITestStatusManager), typeof(ContainerControlledLifetimeManager))]
    public class TestStatusManager : ITestStatusManager
    {
        public Dictionary<(string fix, string rec), TestStatus> CurrentStatus { get; } = new Dictionary<(string fix, string rec), TestStatus>();
        private readonly object m_TestStatusDictionaryLock = new object();

        public event Action TestStatusUpdated;
        public string OutputFilePath => Path.Combine(Paths.RoamingAppdataPath, "LatestTestRunStatus.txt");

        public TestStatusManager(ITestRunner TestRunner)
        {
            // I believe TestRunner will take care to report test as failure if special rec fails
            // TestRunner.FixtureSpecialRecordingFailed += OnTestFailed;
            // TestRunner.FixtureSpecialRecordingSucceded += OnTestPassed;
            TestRunner.TestFailed += OnTestFailed;
            TestRunner.TestPassed += OnTestPassed;
        }

        public IEnumerable<string> GetFormattedTestRunStatus() => CurrentStatus
            .Where(p => p.Value != TestStatus.None)
            .Select(p => $"{p.Value.ToString()}: \"{p.Key.fix}.{p.Key.rec}\"");


        public void OutputTestRunStatusToFile(string path = "")
        {
            if (path.IsEmpty())
                path = OutputFilePath;

            File.WriteAllLines(path, GetFormattedTestRunStatus());
        }

        /// <summary>
        /// Returns fixture status.
        /// If all tests inside are green, returns green.
        /// If at least one is red, fixture will also be red.
        /// </summary>
        public TestStatus GetFixtureStatus(LightTestFixture fixture)
        {
            KeyValuePair<(string fix, string rec), TestStatus>[] statusList;
            lock (m_TestStatusDictionaryLock)
            {
                statusList = CurrentStatus.ToArray();
            }

            var status = TestStatus.Passed;
            foreach (var pair in statusList)
            {
                if (pair.Key.fix != fixture.Name)
                    continue;

                // If at least one test failed, mark whole fixture as failed
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

        private void SetStatus((string fix, string rec) tuple, TestStatus status)
        {
            if (CurrentStatus.ContainsKey(tuple))
                CurrentStatus[tuple] = status;
            else
                CurrentStatus.Add(tuple, status);
        }

        #region TestRunner Callbacks

        private void OnTestFailed(LightTestFixture fixture, Recording recording)
        {
            lock (m_TestStatusDictionaryLock)
            {
                var tuple = (fixture.Name, recording.Name);
                SetStatus(tuple, TestStatus.Failed);
                TestStatusUpdated?.Invoke();
            }
        }

        private void OnTestPassed(LightTestFixture fixture, Recording recording)
        {
            lock (m_TestStatusDictionaryLock)
            {
                var tuple = (fixture.Name, recording.Name);
                SetStatus(tuple, TestStatus.Passed);
                TestStatusUpdated?.Invoke();
            }
        }

        #endregion

        /// <summary>
        /// Will check if fixture is known, and if it is not, will update all statuses for that fixture to None.
        /// This method is called from EDITOR. Not used by runtime since fixtures cannot be modified
        /// </summary>
        public void UpdateTestStatusForNewFixtures(IEnumerable<LightTestFixture> fixtures)
        {
            lock (m_TestStatusDictionaryLock)
            {
                foreach (var fixture in fixtures)
                {
                    foreach (var recording in fixture.Tests)
                    {
                        var tuple = (fixture.Name, recording.Name);
                        if (!CurrentStatus.ContainsKey(tuple))
                            CurrentStatus.Add(tuple, TestStatus.None);
                    }
                }
            }
            TestStatusUpdated?.Invoke();
        }

        /// <summary>
        /// This method constructs new dictionary from all tests in old fixture, then iterates new fixture to find matching recordings
        /// Iterating both fixtures would result in O(n^2). With dictionary, it is only O(2n) with some temp memory allocations.
        /// 
        /// This method is called from EDITOR. Not used by runtime since fixtures cannot be modified
        /// </summary>
        public void ResetTestStatusForModifiedTests(LightTestFixture oldFixture, LightTestFixture newFixture)
        {
            // dictionary with count is faster on adding elements. Operation complexity is o(1)
            var newRecordingsDict = new Dictionary<string, Recording>(oldFixture.Tests.Count);
            foreach (var s in oldFixture.Tests)
                newRecordingsDict.Add(s.Name, s);

            var oldRecordings = newFixture.Tests;

            for (int i = 0; i < oldRecordings.Count; ++i)
            {
                var oldRecording = oldRecordings[i];
                Recording newRecording;
                newRecordingsDict.TryGetValue(oldRecording.Name, out newRecording);

                // if new recording is different, mark it's status as None
                if (!oldRecording.Similar(newRecording) && newRecording != null)
                {
                    lock (m_TestStatusDictionaryLock)
                    {
                        var tuple = (oldFixture.Name, newRecording.Name);
                        SetStatus(tuple, TestStatus.None);

                        TestStatusUpdated?.Invoke();
                    }
                }
            }
        }
    }
}
