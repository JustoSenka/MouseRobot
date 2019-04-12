using RobotRuntime.Tests;
using System.Linq;

namespace RobotRuntime.Execution
{
    /// <summary>
    /// Will run parent command and all child commands in order with their appropriate command runner.
    /// Can be overriden to add additional functionality
    /// </summary>
    public class NestedCommandRunner : IRunner
    {
        public TestData TestData { set; get; }
        public NestedCommandRunner() { }

        public void Run(IRunnable runnable)
        {
            var command = runnable as Command;
            var commandNode = TestData.TestFixture.Commands.FirstOrDefault(node => node.value == command);

            if (command == null)
            {
                Logger.Log(LogType.Error, "This runner '" + this + "' is not compatible with this type: '" + runnable.GetType());
                TestData.TestStatus = TestStatus.Failed;
                return;
            }

            TestData.InvokeCallback(commandNode.value.Guid);

            // Before callback can be overriden and completelly change how command look like
            // Make sure to check all nulls before moving forward
            TestData.TestStatus = BeforeRunningParentCommand(ref command);
            if (TestData.IsTestFinished)
                return;

            if (command == null)
            {
                Logger.Log(LogType.Error, "Parent command was null when trying to run it in: " + GetType());
                TestData.TestStatus = TestStatus.Failed;
                return;
            }

            TestData.TestStatus = RunParentCommand(command);
            if (TestData.IsTestFinished)
                return;

            TestData.TestStatus = RunChildCommands(commandNode.Select(n => n.value).ToArray());
            if (TestData.IsTestFinished)
                return;
        }


        /// <summary>
        /// Runs all child commands with appropriate command runners.
        /// Return TestStatus.None for tests to continue execution.
        /// </summary>
        protected virtual TestStatus RunChildCommands(Command[] commands)
        {
            if (TestData.IsTestFinished)
                return TestData.TestStatus;

            for (int i = 0; i < commands.Length; i++)
            {
                if (TestData.IsTestFinished)
                    return TestData.TestStatus;

                var runner = TestData.RunnerFactory.GetFor(commands[i].GetType());

                TestData.InvokeCallback(commands[i].Guid);

                // Before callback can be overriden and completelly change how command/runner look like
                // Make sure to check all nulls before moving forward
                TestData.TestStatus = BeforeRunningChildCommand(ref runner, ref commands[i]);
                if (TestData.IsTestFinished)
                    return TestData.TestStatus;

                if (runner == null || commands[i] == null)
                {
                    if (runner == null)
                        Logger.Log(LogType.Error, "Runner was null when trying to run it in: " + GetType());
                    if (commands[i] == null)
                        Logger.Log(LogType.Error, "Child command was null when trying to run it in: " + GetType());

                    return TestData.TestStatus;
                }

                TestData.TestStatus = RunChildCommand(runner, commands[i]);
                if (TestData.IsTestFinished)
                    return TestData.TestStatus;
            }

            return TestData.TestStatus;
        }

        /// <summary>
        /// Overriding this method allows altering parent command, giving completely different command to execute, or faking
        /// Method itself is empty.
        /// Return TestStatus.None for tests to continue execution.
        /// </summary>
        protected virtual TestStatus BeforeRunningParentCommand(ref Command command) => TestStatus.None;

        /// <summary>
        /// Calls Run method on the runner with specified command.
        /// Return TestStatus.None for tests to continue execution.
        /// </summary>
        protected virtual TestStatus RunParentCommand(Command command)
        {
            command.Run(TestData);
            return TestData.TestStatus;
        }

        /// <summary>
        /// Overriding this method allows altering how child commands are run, giving completely different command to execute, or faking
        /// Method itself is empty
        /// Return TestStatus.None for tests to continue execution.
        /// </summary>
        protected virtual TestStatus BeforeRunningChildCommand(ref IRunner runner, ref Command command) => TestStatus.None;

        /// <summary>
        /// Calls Run method on the runner with specified command.
        /// Return TestStatus.None for tests to continue execution.
        /// </summary>
        protected virtual TestStatus RunChildCommand(IRunner runner, Command command)
        {
            runner.Run(command);
            return TestData.TestStatus;
        }
    }
}
