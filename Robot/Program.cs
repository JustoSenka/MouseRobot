using RobotRuntime;
using System.Reflection;
using Unity;

namespace Robot
{
    public static class Program
    {
        public static void RegisterInterfaces(IUnityContainer Container)
        {
            //Container.RegisterInstance(typeof(IUnityContainer), Container, new ContainerControlledLifetimeManager());

            ContainerUtils.Register(Container, Assembly.GetExecutingAssembly());
        }
    }
}
