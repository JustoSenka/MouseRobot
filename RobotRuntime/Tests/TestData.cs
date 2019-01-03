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
        public bool ShouldFailTest;

        public TestData() { }

        public TestData(IRunnerFactory RunnerFactory, LightRecording TestFixture, CommandRunningCallback Callback,
            bool ShouldCancelRun, bool ShouldFailTest)
        {
            this.RunnerFactory = RunnerFactory;
            this.TestFixture = TestFixture;
            this.CommandRunningCallback = Callback;
            this.ShouldCancelRun = ShouldCancelRun;
            this.ShouldFailTest = ShouldFailTest;
        }

        public void InvokeCallback(Guid guid)
        {
            CommandRunningCallback?.Invoke(guid);
        }
    }
}
