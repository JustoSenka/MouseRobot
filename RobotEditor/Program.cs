using Robot;
using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotEditor.PropertyUtils;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Unity;

namespace RobotEditor
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Trace.Listeners.Clear();
            Debug.Listeners.Clear();
            Trace.Listeners.Add(new TraceListenerWhichLogs());
            Debug.Listeners.Add(new TraceListenerWhichLogs());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var container = new UnityContainer();
            RobotRuntime.Program.RegisterInterfaces(container);
            Robot.Program.RegisterInterfaces(container);
            RegisterInterfaces(container);

            Logger.Instance = container.Resolve<ILogger>();

            ContainerUtils.PassStaticDependencies(container, typeof(RobotRuntime.Program).Assembly);
            ContainerUtils.PassStaticDependencies(container, typeof(Robot.Program).Assembly);
            ContainerUtils.PassStaticDependencies(container, typeof(Program).Assembly);

            container.Resolve<ISolutionManager>(); // not referenced by anything

            var projectIsCreated = SetupProjectPath(container, args);

            if (projectIsCreated)
            {
                var mainForm = container.Resolve<IMainForm>();
                Application.Run(mainForm as Form);
            }

            Application.Exit();
        }

        private static bool SetupProjectPath(IUnityContainer container, string[] args)
        {
            var projectManager = container.Resolve<IProjectManager>();
            var projectDialog = container.Resolve<IProjectSelectionDialog>();

            if (args.Length > 0)
                projectManager.InitProject(args[0]);
            else if (projectManager.LastKnownProjectPaths.Count > 0)
                projectManager.InitProject(projectManager.LastKnownProjectPaths[0]);
            else
            {
                return projectDialog.InitProjectWithDialog();
            }

            return true;
        }

        public static void RegisterInterfaces(IUnityContainer Container)
        {
            //Container.RegisterInstance(typeof(IUnityContainer), Container, new ContainerControlledLifetimeManager());

            ContainerUtils.Register(Container, Assembly.GetExecutingAssembly());
        }
    }
}
