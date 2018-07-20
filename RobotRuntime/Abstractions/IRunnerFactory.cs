using System;
using RobotRuntime.Execution;
using RobotRuntime.Tests;

namespace RobotRuntime.Abstractions
{
    public interface IRunnerFactory
    {
        void PassDependencies(TestData TestData);

        IRunner GetFor(Type type);
        bool DoesRunnerSupportType(Type runnerType, Type supportedType);
    }
}