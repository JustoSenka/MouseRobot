using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Tests;

namespace RobotRuntime.Execution
{
    public class RunRecordingCommandRunner : IRunner
    {
        public TestData TestData { set; get; }

        private IRuntimeAssetManager RuntimeAssetManager;
        public RunRecordingCommandRunner(IRuntimeAssetManager RuntimeAssetManager)
        {
            this.RuntimeAssetManager = RuntimeAssetManager;
        }

        public void Run(IRunnable runnable)
        {
            if (!TestData.RunnerFactory.DoesRunnerSupportType(this.GetType(), runnable.GetType()))
            {
                Logger.Log(LogType.Error, "This runner '" + this + "' is not compatible with this type: '" + runnable.GetType());
                return;
            }

            if (runnable is CommandRunRecording command)
            {
                TestData.InvokeCallback(command.Guid);
                var runner = TestData.RunnerFactory.GetFor(typeof(LightRecording));

                var oldRecording = TestData.TestFixture;

                var recording = RuntimeAssetManager.GetAsset<LightRecording>(command.Asset);
                runner.Run(recording);

                TestData.TestFixture = oldRecording;
            }
        }
    }
}
