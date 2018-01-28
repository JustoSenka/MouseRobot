using RobotRuntime.Abstractions;
using RobotRuntime.Assets;
using Unity;
using Unity.Lifetime;
using RobotRuntime.Graphics;
using RobotRuntime.Settings;
using RobotRuntime.Execution;

namespace RobotRuntime
{
    public class Program
    {
        static void Main(string[] args)
        {
            //if (args[0])

            var container = new UnityContainer();
            RegisterInterfaces(container);
        }

        public static void RegisterInterfaces(UnityContainer Container)
        {
            Container.RegisterType<IAssetGuidManager, AssetGuidManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IFeatureDetectionThread, FeatureDetectionThread>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRuntimeSettings, RuntimeSettings>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScreenStateThread, ScreenStateThread>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITestRunner, TestRunner>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRunnerFactory, RunnerFactory>(new ContainerControlledLifetimeManager());
        }
    }
}
