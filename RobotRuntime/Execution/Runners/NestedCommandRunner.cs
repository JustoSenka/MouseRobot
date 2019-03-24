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
                TestData.ShouldFailTest = true;
                return;
            }

            TestData.InvokeCallback(commandNode.value.Guid);

            // Before callback can be overriden and completelly change how command look like
            // Make sure to check all nulls before moving forward
            if (BeforeRunningParentCommand(ref command))
            {
                TestData.ShouldFailTest = true;
                return;
            }

            if (command == null)
            {
                Logger.Log(LogType.Error, "Parent command was null when trying to run it in: " + GetType());
                TestData.ShouldFailTest = true;
                return;
            }

            if (RunParentCommand(command))
            {
                TestData.ShouldFailTest = true;
                return;
            }

            if (RunChildCommands(commandNode.Select(n => n.value).ToArray()))
            {
                TestData.ShouldFailTest = true;
                return;
            }
        }


        /// <summary>
        /// Runs all child commands with appropriate command runners.
        /// Return true if test should be cancelled and marked as failed.
        /// </summary>
        protected virtual bool RunChildCommands(Command[] commands)
        {
            if (TestData.ShouldCancelRun || TestData.ShouldFailTest)
                return true;

            for (int i = 0; i < commands.Length; i++)
            {
                if (TestData.ShouldCancelRun)
                    return true;

                var runner = TestData.RunnerFactory.GetFor(commands[i].GetType());

                TestData.InvokeCallback(commands[i].Guid);

                // Before callback can be overriden and completelly change how command/runner look like
                // Make sure to check all nulls before moving forward
                if (BeforeRunningChildCommand(ref runner, ref commands[i]))
                    return true;

                if (runner == null || commands[i] == null)
                {
                    if (runner == null)
                        Logger.Log(LogType.Error, "Runner was null when trying to run it in: " + GetType());
                    if (commands[i] == null)
                        Logger.Log(LogType.Error, "Child command was null when trying to run it in: " + GetType());

                    return true;
                }

                if (RunChildCommand(runner, commands[i]))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Overriding this method allows altering parent command, giving completely different command to execute, or faking
        /// Method itself is empty.
        /// Return true if test should be cancelled and marked as failed.
        /// </summary>
        protected virtual bool BeforeRunningParentCommand(ref Command command) => false;

        /// <summary>
        /// Calls Run method on the runner with specified command.
        /// Return true if test should be cancelled and marked as failed.
        /// </summary>
        protected virtual bool RunParentCommand(Command command)
        {
            command.Run(TestData);
            return TestData.ShouldFailTest;
        }

        /// <summary>
        /// Overriding this method allows altering how child commands are run, giving completely different command to execute, or faking
        /// Method itself is empty
        /// Return true if test should be cancelled and marked as failed.
        /// </summary>
        protected virtual bool BeforeRunningChildCommand(ref IRunner runner, ref Command command) => false;

        /// <summary>
        /// Calls Run method on the runner with specified command.
        /// Return true if test should be cancelled and marked as failed.
        /// </summary>
        protected virtual bool RunChildCommand(IRunner runner, Command command)
        {
            runner.Run(command);
            return TestData.ShouldFailTest;
        }
    }
}
