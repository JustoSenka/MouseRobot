using CommandLine;
using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using System.Diagnostics;
using System.Reflection;
using Unity;
using Unity.Lifetime;

namespace RobotRuntime
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Trace.Listeners.Clear();
            Debug.Listeners.Clear();
            Trace.Listeners.Add(new TraceListenerWhichLogs());
            Debug.Listeners.Add(new TraceListenerWhichLogs());

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

            ContainerUtils.Register(Container, Assembly.GetExecutingAssembly());
            ContainerUtils.RegisterPrimitiveTypes(Container);
        }
    }
}
