using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Tests;

namespace RobotRuntime.Execution
{
    public class RunScriptCommandRunner : IRunner
    {
        public TestData TestData { set; get; }

        private IRuntimeAssetManager RuntimeAssetManager;
        public RunScriptCommandRunner(IRuntimeAssetManager RuntimeAssetManager)
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

            if (runnable is CommandRunScript command)
            {
                TestData.InvokeCallback(command.Guid);
                var runner = TestData.RunnerFactory.GetFor(typeof(LightScript));

                var oldScript = TestData.TestFixture;

                var script = RuntimeAssetManager.GetAsset<LightScript>(command.Asset);
                runner.Run(script);

                TestData.TestFixture = oldScript;
            }
        }
    }
}
