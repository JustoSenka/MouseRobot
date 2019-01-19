using RobotRuntime.Abstractions;
using RobotRuntime.Assets;
using Unity;
using Unity.Lifetime;
using RobotRuntime.Graphics;
using RobotRuntime.Settings;
using RobotRuntime.Execution;
using RobotRuntime.Perf;
using RobotRuntime.Scripts;
using System;
using RobotRuntime.Logging;
using Unity.Injection;
using System.Linq;

namespace RobotRuntime
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 2)
                return;

            Environment.CurrentDirectory = args[0];

            var container = new UnityContainer();
            RegisterInterfaces(container);

            Logger.Instance = container.Resolve<ILogger>();

            container.Resolve<IScriptLoader>(); // Not referenced by runtime

            var projectManager = container.Resolve<IRuntimeProjectManager>();
            projectManager.InitProject(args[0]);

            var testRunner = container.Resolve<ITestRunner>();
            testRunner.LoadSettings();
            testRunner.StartRecording(args[1]).Wait();
        }


        public static void RegisterInterfaces(UnityContainer Container)
        {
            Container.RegisterInstance(typeof(IUnityContainer), Container, new ContainerControlledLifetimeManager());
            Container.RegisterType<IAssetGuidManager, AssetGuidManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRuntimeAssetManager, RuntimeAssetManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IFeatureDetectionThread, FeatureDetectionThread>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRuntimeSettings, RuntimeSettings>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScreenStateThread, ScreenStateThread>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITestRunner, TestRunner>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRunnerFactory, RunnerFactory>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IProfiler, Profiler>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IFeatureDetectorFactory, FeatureDetectorFactory>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScriptLoader, ScriptLoaderNoDomain>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRuntimeProjectManager, RuntimeProjectManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IStatusManager, StatusManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ILogger, Logger>(new ContainerControlledLifetimeManager());

            Container.RegisterType(typeof(ITypeCollector<>), typeof(TypeCollector<>));
            Container.RegisterType(typeof(ITypeObjectCollector<>), typeof(TypeObjectCollector<>));

            // Kinda optional, will not fail because of integers, guids or booleans in constructors. Not really used currently.
            // In perfect scenarion, Unity should not try to resolve class which has primitives in constructors
            RegisterPrimitiveTypes(Container);
        }

        private static void RegisterPrimitiveTypes(UnityContainer Container)
        {
            var allPrimitiveTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => t.IsPrimitive);
            foreach (var t in allPrimitiveTypes)
                Container.RegisterType(t, new InjectionFactory((c) => default));

            Container.RegisterType<Guid>(new InjectionConstructor(Guid.Empty.ToString()));
            Container.RegisterType<DateTime>(new InjectionConstructor((long)0));
        }
    }
}
