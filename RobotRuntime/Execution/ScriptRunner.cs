using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Utils;
using System;
using System.Linq;

namespace RobotRuntime.Execution
{
    public class ScriptRunner : IRunner
    {
        private CommandRunningCallback m_Callback;
        private ValueWrapper<bool> m_ShouldCancelRun;

        private IRunnerFactory RunnerFactory;
        public ScriptRunner()
        {

        }

        public void PassDependencies(IRunnerFactory RunnerFactory, LightScript TestFixture, CommandRunningCallback Callback, ValueWrapper<bool> ShouldCancelRun)
        {
            this.RunnerFactory = RunnerFactory;
            m_Callback = Callback;
            m_ShouldCancelRun = ShouldCancelRun;
        }

        public void Run(IRunnable runnable)
        {
            if (!RunnerFactory.DoesRunnerSupportType(this.GetType(), runnable.GetType()))
            {
                Logger.Log(LogType.Error, "This runner '" + this + "' is not compatible with this type: '" + runnable.GetType());
                return;
            }

            var script = runnable as LightScript;
            RunnerFactory.PassDependencies(script, m_Callback, m_ShouldCancelRun);

            // making shallow copy of commands collection, so it doesn't crash when test tries to modify itself while running
            foreach (var node in script.Commands.ToList())
            {
                if (m_ShouldCancelRun.Value)
                {
                    Logger.Log(LogType.Log, "Script run was cancelled.");
                    return;
                }

                var runner = RunnerFactory.GetFor(node.value.GetType());
                runner.Run(node.value);
            }
        }
    }
}
