using RobotRuntime.Abstractions;
using RobotRuntime.Execution;

namespace RobotRuntime.Tests
{
    public class TestData
    {
        public IRunnerFactory RunnerFactory;
        public LightScript TestFixture;
        public CommandRunningCallback CommandRunningCallback;
        public bool ShouldCancelRun;
        public bool ShouldFailTest;

        public TestData() { }

        public TestData(IRunnerFactory RunnerFactory, LightScript TestFixture, CommandRunningCallback Callback,
            bool ShouldCancelRun, bool ShouldFailTest)
        {
            this.RunnerFactory = RunnerFactory;
            this.TestFixture = TestFixture;
            this.CommandRunningCallback = Callback;
            this.ShouldCancelRun = ShouldCancelRun;
            this.ShouldFailTest = ShouldFailTest;
        }
    }
}
