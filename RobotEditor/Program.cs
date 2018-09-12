using RobotEditor.Abstractions;
using System;
using Unity;
using Unity.Lifetime;
using RobotEditor.Windows;
using System.Windows.Forms;
using Robot;
using RobotEditor.Editor;
using Robot.Plugins;
using RobotRuntime.Abstractions;
using RobotRuntime;
using RobotEditor.Hierarchy;
using RobotEditor.PropertyUtils;

namespace RobotEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var container = new UnityContainer();
            RobotRuntime.Program.RegisterInterfaces(container);
            Robot.Program.RegisterInterfaces(container);
            RegisterInterfaces(container);

            container.Resolve<PropertyDependencyProvider>(); // Needed for UITypeEditor and StringConverter to work correctly, since .NET initialized them

            Logger.Instance = container.Resolve<ILogger>();

            var projectIsCreated = SetupProjectPath(container);

            if (projectIsCreated)
            {
                var mainForm = container.Resolve<IMainForm>();
                Application.Run(mainForm as Form);
            }

            Application.Exit();
        }

        private static bool SetupProjectPath(IUnityContainer container)
        {
            var projectManager = container.Resolve<IProjectManager>();
            var projectDialog = container.Resolve<IProjectSelectionDialog>();

            if (projectManager.LastKnownProjectPaths.Count > 0)
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
            Container.RegisterType<ITestFixtureWindow, TestFixtureWindow>();
        }
    }
}
