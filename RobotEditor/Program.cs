using Robot;
using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotEditor.Editor;
using RobotEditor.Hierarchy;
using RobotEditor.PropertyUtils;
using RobotEditor.Resources.ScriptTemplates;
using RobotEditor.Windows;
using RobotRuntime;
using RobotRuntime.Abstractions;
using System;
using System.Windows.Forms;
using Unity;
using Unity.Lifetime;

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var container = new UnityContainer();
            RobotRuntime.Program.RegisterInterfaces(container);
            Robot.Program.RegisterInterfaces(container);
            RegisterInterfaces(container);

            Logger.Instance = container.Resolve<ILogger>();

            container.Resolve<PropertyDependencyProvider>(); // Needed for UITypeEditor and StringConverter to work correctly, since .NET initialized them
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
            Container.RegisterType<IMainForm, MainForm>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IAssetsWindow, AssetsWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IHierarchyWindow, HierarchyWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IInspectorWindow, InspectorWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IProfilerWindow, ProfilerWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IPropertiesWindow, PropertiesWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScreenPreviewWindow, ScreenPreviewWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IConsoleWindow, ConsoleWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITestRunnerWindow, TestRunnerWindow>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IHierarchyNodeStringConverter, HierarchyNodeStringConverter>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScreenPaintForm, ScreenPaintForm>(new ContainerControlledLifetimeManager());

            Container.RegisterType<PropertyDependencyProvider, PropertyDependencyProvider>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IProjectSelectionDialog, ProjectSelectionDialog>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScriptTemplates, ScriptTemplates>(new ContainerControlledLifetimeManager());

            Container.RegisterType<ITestFixtureWindow, TestFixtureWindow>();
        }
    }
}
