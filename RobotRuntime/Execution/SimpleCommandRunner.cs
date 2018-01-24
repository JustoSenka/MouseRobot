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

        public SimpleCommandRunner(CommandRunningCallback callback)
        {
            m_Callback = callback;
        }

        public void Run(IRunnable runnable)
        {
            if (!RunnerFactory.DoesRunnerSupportType(this.GetType(), runnable.GetType()))
                throw new ArgumentException("This runner '" + this + "' is not compatible with this type: '" + runnable.GetType());

            var command = runnable as Command;

            m_Callback.Invoke(command);
            command.Run();
        }
    }
}
