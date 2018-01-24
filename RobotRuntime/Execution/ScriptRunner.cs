using RobotRuntime.Commands;
using System;

namespace RobotRuntime.Execution
{
    public class ScriptRunner : IRunner
    {
        private CommandRunningCallback m_Callback;
        public ScriptRunner(CommandRunningCallback callback)
        {
            m_Callback = callback;
        }

        public void Run(IRunnable runnable)
        {
            if (!IsValidFor(runnable.GetType()))
                throw new ArgumentException("This runner '" + this + "' is not compatible with this type: '" + runnable.GetType());

            var script = runnable as LightScript;
            RunnerFactory.ExecutingScript = script;

            foreach (var node in script.Commands)
            {
                var runner = RunnerFactory.CreateFor(node.value.GetType());
                runner.Run(node.value);

                /*if (!m_Run)
                    break;*/
            }
        }

        bool IRunner.IsValidForType(Type type)
        {
            return IsValidFor(type);
        }

        public static bool IsValidFor(Type type)
        {
            return type == typeof(LightScript);
        }
    }
}
