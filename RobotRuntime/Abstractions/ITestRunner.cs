﻿using System;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;

namespace RobotRuntime.Abstractions
{
    public interface ITestRunner
    {
        TestData TestData { get; }

        event Action TestRunStart;
        event Action TestRunEnd;

        event Action<LightTestFixture> FixtureIsBeingRun;
        event Action<LightTestFixture, Recording> TestIsBeingRun;

        event Action<LightTestFixture, Recording> FixtureSpecialScripFailed;
        event Action<LightTestFixture, Recording> FixtureSpecialRecordingSucceded;
        event Action<LightTestFixture, Recording> TestPassed;
        event Action<LightTestFixture, Recording> TestFailed;

        void StartRecording(string projectPath, string recordingName);
        void StartTests(string projectPath, string testFilter = ".");

        void StartRecording(LightRecording lightRecording);
        void StartTests(string testFilter = ".");

        void Stop();
    }
}