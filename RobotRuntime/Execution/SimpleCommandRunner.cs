using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using System;

namespace RobotRuntime.Execution
{
    [SupportedType(typeof(CommandDown))]
    [SupportedType(typeof(CommandMove))]
    [SupportedType(typeof(CommandPress))]
    [SupportedType(typeof(CommandRelease))]
    [SupportedType(typeof(CommandSleep))]
    public class SimpleCommandRunner : IRunner
    {
        private CommandRunningCallback m_Callback;

        private IRunnerFactory RunnerFactory;
        public SimpleCommandRunner(IRunnerFactory RunnerFactory, CommandRunningCallback callback)
        {
            m_Callback = callback;
            this.RunnerFactory = RunnerFactory;
        }

        public void Run(IRunnable runnable)
        {
            if (!RunnerFactory.DoesRunnerSupportType(this.GetType(), runnable.GetType()))
            {
                Logger.Log(LogType.Error, "This runner '" + this + "' is not compatible with this type: '" + runnable.GetType());
                return;
            }

            var command = runnable as Command;

            m_Callback.Invoke(command);
            command.Run();
        }
    }
}
