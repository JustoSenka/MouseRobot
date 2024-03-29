﻿using System;
using System.Threading.Tasks;
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

        event Action<LightTestFixture, Recording> FixtureSpecialRecordingFailed;
        event Action<LightTestFixture, Recording> FixtureSpecialRecordingSucceded;
        event Action<LightTestFixture, Recording> TestPassed;
        event Action<LightTestFixture, Recording> TestFailed;

        void LoadSettings();

        Task StartRecording(string recordingName);
        Task StartRecording(LightRecording lightRecording);
        Task StartTests(string testFilter = ".");

        void Stop();
    }
}