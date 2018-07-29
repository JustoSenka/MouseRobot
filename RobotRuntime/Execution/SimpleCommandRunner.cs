using RobotRuntime.Abstractions;
using RobotRuntime.Tests;

namespace RobotRuntime.Execution
{
    public class SimpleCommandRunner : IRunner
    {
        public TestData TestData { set; get; }

        public SimpleCommandRunner()
        {
            
        }
        
        public void Run(IRunnable runnable)
        {
            if (!TestData.RunnerFactory.DoesRunnerSupportType(this.GetType(), runnable.GetType()))
            {
                Logger.Log(LogType.Error, "This runner '" + this + "' is not compatible with this type: '" + runnable.GetType());
                return;
            }

            var command = runnable as Command;

            TestData.CommandRunningCallback?.Invoke(command);
            command.Run(TestData);
        }
    }
}
