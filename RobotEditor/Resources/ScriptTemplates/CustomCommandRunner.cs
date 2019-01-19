using RobotRuntime.Abstractions;
using RobotRuntime;
using RobotRuntime.Execution;
using RobotRuntime.Tests;

namespace RobotEditor.Resources.ScriptTemplates
{
    public class CustomCommandRunner : IRunner
    {
        private CommandRunningCallback m_Callback;

        public CustomCommandRunner()
        {
            // Constructor actually can ask for other managers if needed, like IHierarchyManager etc.
        }

        // Contains useful data:
        // if ShouldCancelRun is set to true, the test run will stop
        // RunnerFactory is useful if command is nested and need to get other runners
        // TestFixture can be used to get nested commands: LightRecording.Commands.GetNodeFromValue(command)
        public TestData TestData { set; get; }

        public void Run(IRunnable runnable)
        {
            Logger.Log(LogType.Debug, "This is custom command runner which only logs this message");
			
            var command = runnable as Command;

            // Callbacks are necessary so hierarchy could highlight currently running command
            TestData.InvokeCallback(command.Guid);

            // TODO: RUN METHOD
            // Optional, depends on the commands it can run
            command.Run(TestData);
        }
    }
}
