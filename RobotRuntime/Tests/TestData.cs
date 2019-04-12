using RobotRuntime.Abstractions;
using RobotRuntime.Execution;
using System;

namespace RobotRuntime.Tests
{
    public class TestData
    {
        public IRunnerFactory RunnerFactory;
        public LightRecording TestFixture;
        public event CommandRunningCallback CommandRunningCallback;
        public bool ShouldCancelRun;
        public TestStatus TestStatus;

        public TestData() { }

        public TestData(IRunnerFactory RunnerFactory, LightRecording TestFixture, CommandRunningCallback Callback,
            bool ShouldCancelRun, TestStatus TestStatus)
        {
            this.RunnerFactory = RunnerFactory;
            this.TestFixture = TestFixture;
            this.CommandRunningCallback = Callback;
            this.ShouldCancelRun = ShouldCancelRun;
            this.TestStatus = TestStatus;
        }

        public bool IsTestFinished => ShouldCancelRun || TestStatus != TestStatus.None;

        public void InvokeCallback(Guid guid)
        {
            CommandRunningCallback?.Invoke(guid);
        }
    }
}
