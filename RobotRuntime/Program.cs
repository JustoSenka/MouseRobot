using RobotRuntime.Abstractions;
using RobotRuntime.Assets;
using Unity;
using Unity.Lifetime;
using RobotRuntime.Graphics;
using RobotRuntime.Settings;
using RobotRuntime.Execution;
using RobotRuntime.Perf;
using RobotRuntime.Plugins;
using System;

namespace RobotRuntime
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
                return;

            Environment.CurrentDirectory = args[0];

            var container = new UnityContainer();
            container.RegisterInstance(typeof(IUnityContainer), container, new ContainerControlledLifetimeManager());
            RegisterInterfaces(container);

            var testRunner = container.Resolve<ITestRunner>();
            testRunner.Start(args[0], args[1]);
        }


        public static void RegisterInterfaces(UnityContainer Container)
        {
            Container.RegisterInstance(typeof(IUnityContainer), Container, new ContainerControlledLifetimeManager());
            Container.RegisterType<IAssetGuidManager, AssetGuidManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IFeatureDetectionThread, FeatureDetectionThread>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRuntimeSettings, RuntimeSettings>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScreenStateThread, ScreenStateThread>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITestRunner, TestRunner>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRunnerFactory, RunnerFactory>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IProfiler, Profiler>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IFeatureDetectorFactory, FeatureDetectorFactory>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IPluginLoader, PluginLoaderNoDomain>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ILogger, Logger>(new ContainerControlledLifetimeManager());

            // Container.RegisterType<IPluginDomainManager, FeatureDetectorFactory>(new ContainerControlledLifetimeManager());

            Logger.Instance = Container.Resolve<ILogger>();
        }
    }
}
