using RobotRuntime.Abstractions;
using RobotRuntime.Tests;
using System;
using System.Linq;

namespace RobotRuntime.Execution
{
    public class RunnerFactory : IRunnerFactory
    {
        private TestData m_TestData;

        private ILogger Logger;
        private ICustomTypeObjectCollector<IRunner> TypeCollector;
        public RunnerFactory(ILogger Logger, ICustomTypeObjectCollector<IRunner> TypeCollector)
        {
            this.Logger = Logger;
            this.TypeCollector = TypeCollector;

            TypeCollector.NewTypesAppeared += UpdateDependencies;
        }

        public void PassDependencies(TestData TestData)
        {
            this.m_TestData = TestData;
            TestData.RunnerFactory = this;

            foreach (var runner in TypeCollector.AllObjects)
                runner.TestData = TestData;
        }

        private void UpdateDependencies()
        {
            foreach (var runner in TypeCollector.AllObjects)
                runner.TestData = m_TestData;
        }

        public IRunner GetFor(Type commandType)
        {
            var runnerType = GetRunnerTypeForCommand(commandType);
            if (runnerType != null)
            {
                return TypeCollector.TypeObjectMap[runnerType];
            }
            else
            {
                Logger.Logi(LogType.Error, "Threre is no Runner registered that would support type: " + commandType);
                return TypeCollector.TypeObjectMap[typeof(SimpleCommandRunner)];
            }
        }

        public bool DoesRunnerSupportType(Type runnerType, Type commandType)
        {
            return GetRunnerTypeForCommand(commandType) != null;
        }

        private Type GetRunnerTypeForCommand(Type commandType)
        {
            var attribute = commandType.GetCustomAttributes(false).OfType<RunnerTypeAttribute>().FirstOrDefault();
            return attribute != null ? attribute.type : null;
        }
    }
}
