using System;
using RobotRuntime.Utils;
using RobotRuntime.Execution;

namespace RobotRuntime.Abstractions
{
    public interface IRunnerFactory
    {
        CommandRunningCallback Callback { get; set; }
        ValueWrapper<bool> CancellingPointerPlaceholder { get; set; }
        LightScript ExecutingScript { get; set; }

        IRunner CreateFor(Type type);
        bool DoesRunnerSupportType(Type runnerType, Type supportedType);
    }
}