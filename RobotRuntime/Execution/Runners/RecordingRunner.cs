using RobotRuntime.Abstractions;
using RobotRuntime.Tests;
using System.Linq;

namespace RobotRuntime.Execution
{
    public class RecordingRunner : IRunner
    {
        public TestData TestData { set; get; }

        private ILogger Logger;
        public RecordingRunner(ILogger Logger)
        {
            this.Logger = Logger;
        }

        public void Run(IRunnable runnable)
        {
            var recording = runnable as LightRecording;

            if (Logger.AssertIf(recording == null, "Recording is invalid: " + runnable))
                return;

            TestData.TestFixture = recording;
            TestData.RunnerFactory.PassDependencies(TestData);

            Logger.Logi(LogType.Debug, "Recording is being run: " + recording.Name);

            // making shallow copy of commands collection, so it doesn't crash when test tries to modify itself while running
            foreach (var node in recording.Commands.ToList())
            {
                if (TestData.ShouldCancelRun)
                {
                    Logger.Logi(LogType.Log, "Recording run was cancelled.");
                    return;
                }

                var runner = TestData.RunnerFactory.GetFor(node.value.GetType());
                runner.Run(node.value);

                if (TestData.ShouldFailTest)
                    return;
            }
        }
    }
}
