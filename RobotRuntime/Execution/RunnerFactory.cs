using RobotRuntime.Utils;
using System;
using System.Linq;

namespace RobotRuntime.Execution
{
    public static class RunnerFactory
    {
        public static CommandRunningCallback Callback { get; set; }
        public static LightScript ExecutingScript { get; set; }
        public static ValueWrapper<bool> CancellingPointerPlaceholder { get; set; }

        public static IRunner CreateFor(Type type)
        {
            if (DoesRunnerSupportType(typeof(SimpleCommandRunner), type))
                return new SimpleCommandRunner(Callback);

            else if (DoesRunnerSupportType(typeof(ImageCommandRunner), type))
                return new ImageCommandRunner(ExecutingScript, Callback, CancellingPointerPlaceholder);

            else if (DoesRunnerSupportType(typeof(ScriptRunner), type))
                return new ScriptRunner(Callback, CancellingPointerPlaceholder);

            else
                throw new Exception("Threre is no Runner registered that would support type: " + type);

        }

        // TODO: currently runner is created for every single command, and this is executed quite often, might be slow. Consider caching everyhing in Dictionary
        public static bool DoesRunnerSupportType(Type runnerType, Type supportedType)
        {
            return runnerType.GetCustomAttributes(false).OfType<SupportedTypeAttribute>().Where(a => a.type == supportedType).Count() > 0;
        }
    }
}
