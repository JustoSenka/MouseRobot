using System;
using RobotRuntime.Utils;
using RobotRuntime.Execution;

namespace RobotRuntime.Abstractions
{
    public interface IRunnerFactory
    {
        void PassDependencies(LightScript TestFixture, CommandRunningCallback Callback, ValueWrapper<bool> ShouldCancelRun);

        IRunner GetFor(Type type);
        bool DoesRunnerSupportType(Type runnerType, Type supportedType);
    }
}