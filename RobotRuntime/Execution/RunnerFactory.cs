using System;

namespace RobotRuntime.Execution
{
    public static class RunnerFactory
    {
        public static CommandRunningCallback Callback { get; set; }
        public static LightScript ExecutingScript { get; set; }

        public static IRunner CreateFor(Type type)
        {
            return SimpleCommandRunner.IsValidFor(type) ? (IRunner)new SimpleCommandRunner(Callback) :
                ImageCommandRunner.IsValidFor(type) ? (IRunner)new ImageCommandRunner(ExecutingScript, Callback) : null;
        }
    }
}
