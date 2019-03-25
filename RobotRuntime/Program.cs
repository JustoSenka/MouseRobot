using CommandLine;
using RobotRuntime.Abstractions;
using RobotRuntime.Assets;
using RobotRuntime.Execution;
using RobotRuntime.Graphics;
using RobotRuntime.Logging;
using RobotRuntime.Perf;
using RobotRuntime.Scripts;
using RobotRuntime.Settings;
using RobotRuntime.Tests;
using System;
using System.IO;
using System.Linq;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace RobotRuntime
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // FIRST THING (Register all the dependencies)
            var container = new UnityContainer();
            RegisterInterfaces(container);
            Logger.Instance = container.Resolve<ILogger>();

            // SECOND THING (Parse command line arguments)
            var o = Parser.Default.ParseArguments<Options>(args).MapResult(op => op, errors =>
            {
                foreach (var e in errors)
                    Logger.Log(LogType.Error, "Cannot parse command line argument: " + e);

                return default;
            });

            container.Resolve<IScriptLoader>(); // Not referenced by runtime
            var TestStatusManager = container.Resolve<ITestStatusManager>(); // Not referenced by runtime

            // Initialize project
            var projectManager = container.Resolve<IRuntimeProjectManager>();
            projectManager.InitProject(o.ProjectPath);

            // Initialize test runner
            var testRunner = container.Resolve<ITestRunner>();
            testRunner.LoadSettings();

            // Run tests or recordings
            if (o.Recording != "")
                testRunner.StartRecording(o.Recording).Wait();
            else
                testRunner.StartTests(o.TestFilter).Wait();

            // Output test run status to specified or default file path
            TestStatusManager.OutputTestRunStatusToFile(o.Output);
        }

        private class Options
        {
            [Option('p', "projectPath", Required = true, HelpText = "Project path.")]
            public string ProjectPath { get; set; }

            [Option('f', "filter", Required = false, HelpText = "Regex Test Filter for which tests to run.", Default = ".")]
            public string TestFilter { get; set; }

            [Option('r', "recording", Required = false, HelpText = "Run single recording by path.", Default = "")]
            public string Recording { get; set; }

            [Option('o', "output", Required = false, HelpText = "Output file path with extension.", Default = "TestResults.txt")]
            public string Output { get; set; }
        }

        public static void RegisterInterfaces(UnityContainer Container)
        {
            Container.RegisterInstance(typeof(IUnityContainer), Container, new ContainerControlledLifetimeManager());
            Container.RegisterType<IAssetGuidManager, AssetGuidManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRuntimeAssetManager, RuntimeAssetManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRuntimeSettings, RuntimeSettings>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITestRunner, TestRunner>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRunnerFactory, RunnerFactory>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IProfiler, Profiler>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IFeatureDetectorFactory, FeatureDetectorFactory>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScriptLoader, ScriptLoaderNoDomain>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRuntimeProjectManager, RuntimeProjectManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IStatusManager, StatusManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITestStatusManager, TestStatusManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDetectionManager, DetectionManager>(new ContainerControlledLifetimeManager());

            Container.RegisterType<ILogger, Logger>(new ContainerControlledLifetimeManager());

            Container.RegisterType(typeof(ITypeCollector<>), typeof(TypeCollector<>));
            Container.RegisterType(typeof(ITypeObjectCollector<>), typeof(TypeObjectCollector<>));

            // Registering also primitive types, just in case user code tries to inject primitives via constructor, which is not a good idea
            RegisterPrimitiveTypes(Container);
        }

        /// <summary>
        /// It is here so resolving will not fail because of integers, guids or booleans in constructors. Not really used currently.
        /// In perfect scenarion, Unity should not try to resolve class which has primitives in constructors.
        /// </summary>
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
