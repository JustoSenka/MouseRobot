using RobotRuntime.Commands;
using System;

namespace RobotRuntime.Execution
{
    public class SimpleCommandRunner : IRunner
    {
        private CommandRunningCallback m_Callback;

        public SimpleCommandRunner(CommandRunningCallback callback)
        {
            m_Callback = callback;
        }

        public void Run(IRunnable runnable)
        {
            if (!IsValidFor(runnable.GetType()))
                throw new ArgumentException("This runner '" + this + "' is not compatible with this type: '" + runnable.GetType());

            var command = runnable as Command;

            m_Callback.Invoke(command);
            command.Run();
        }

        bool IRunner.IsValidForType(Type type)
        {
            return IsValidFor(type);
        }

        public static bool IsValidFor(Type type)
        {
            return type == typeof(CommandDown) || type == typeof(CommandMove) || type == typeof(CommandPress) || type == typeof(CommandRelease) || type == typeof(CommandSleep);
        }
    }
}
