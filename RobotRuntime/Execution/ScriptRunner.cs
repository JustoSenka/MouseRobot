using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Utils;
using System;

namespace RobotRuntime.Execution
{
    [SupportedType(typeof(LightScript))]
    public class ScriptRunner : IRunner
    {
        private CommandRunningCallback m_Callback;
        private ValueWrapper<bool> m_ShouldCancelRun;

        private IRunnerFactory RunnerFactory;
        public ScriptRunner(IRunnerFactory RunnerFactory, CommandRunningCallback callback, ValueWrapper<bool> ShouldCancelRun)
        {
            m_Callback = callback;
            m_ShouldCancelRun = ShouldCancelRun;

            this.RunnerFactory = RunnerFactory;
        }

        public void Run(IRunnable runnable)
        {
            if (!RunnerFactory.DoesRunnerSupportType(this.GetType(), runnable.GetType()))
            {
                Logger.Log(LogType.Error, "This runner '" + this + "' is not compatible with this type: '" + runnable.GetType());
                return;
            }

            var script = runnable as LightScript;
            RunnerFactory.ExecutingScript = script;

            foreach (var node in script.Commands)
            {
                if (m_ShouldCancelRun.Value)
                {
                    Logger.Log(LogType.Log, "Script run was cancelled.");
                    return;
                }

                var runner = RunnerFactory.CreateFor(node.value.GetType());
                runner.Run(node.value);
            }
        }
    }
}
