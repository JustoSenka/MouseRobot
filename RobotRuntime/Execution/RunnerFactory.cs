using RobotRuntime.Abstractions;
using RobotRuntime.Tests;
using System;
using System.Linq;
using Unity;
using Unity.Lifetime;

namespace RobotRuntime.Execution
{
    [RegisterTypeToContainer(typeof(IRunnerFactory), typeof(ContainerControlledLifetimeManager))]
    public class RunnerFactory : IRunnerFactory
    {
        private TestData m_TestData;

        private readonly ILogger Logger;
        private readonly ITypeObjectCollector<IRunner> TypeCollector;
        private readonly IUnityContainer Container;
        public RunnerFactory(ILogger Logger, ITypeObjectCollector<IRunner> TypeCollector, IUnityContainer Container)
        {
            this.Logger = Logger;
            this.TypeCollector = TypeCollector;
            this.Container = Container;

            TypeCollector.NewTypesAppeared += UpdateDependencies;
            UpdateDependencies();
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
                var allTypes = TypeCollector.AllTypes.ToArray();
                var allObjs = TypeCollector.AllObjects.ToArray();

                var type = allTypes[5 % allTypes.Length];
                var obj = Container.Resolve(runnerType);
                var objFromType = Container.Resolve(obj.GetType()); 

                var obj2 = Container.Resolve(type);

                var a = runnerType == type;
                var b = runnerType.Equals(type);


                return TypeCollector.TypeObjectMap[runnerType];
            }
            else
            {
                // Will spew errors for all SimpleCommands which don't need to have the attribute
                // Logger.Logi(LogType.Error, "Threre is no Runner registered that would support type: " + commandType);
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
            return attribute?.type;
        }
    }
}
