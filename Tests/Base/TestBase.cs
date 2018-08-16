using RobotRuntime;
using RobotRuntime.Abstractions;
using Unity;
using Unity.Lifetime;

namespace Tests
{
    public static class TestBase
    {
        public static IUnityContainer ConstructContainerForTests()
        {
            var container = new UnityContainer();
            RobotRuntime.Program.RegisterInterfaces(container);
            Robot.Program.RegisterInterfaces(container);

            container.RegisterType<ILogger, FakeLogger>(new ContainerControlledLifetimeManager());
            Logger.Instance = container.Resolve<ILogger>();

            return container;
        }
    }
}
