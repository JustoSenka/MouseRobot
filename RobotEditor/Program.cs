using RobotEditor.Abstractions;
using System;
using Unity;
using Unity.Lifetime;
using RobotEditor.Windows;
using System.Windows.Forms;
using RobotEditor.Scripts;
using RobotEditor.Windows.Base;
using RobotEditor.Drawing;
using Unity.Injection;

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
            RegisterInterfaces(container);
            Robot.Program.RegisterInterfaces(container);
            RobotRuntime.Program.RegisterInterfaces(container);

            var mainForm = container.Resolve<IMainForm>();

            var a = container.Resolve<VisualizationPainter>();

            Application.Run(mainForm as Form);
        }

        public static void RegisterInterfaces(UnityContainer Container)
        {
            Container.RegisterInstance(typeof(IUnityContainer), Container, new ContainerControlledLifetimeManager());
            Container.RegisterType<IMainForm, MainForm>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IAssetsWindow, AssetsWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IHierarchyWindow, HierarchyWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IInspectorWindow, InspectorWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IProfilerWindow, ProfilerWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IPropertiesWindow, PropertiesWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScreenPreviewWindow, ScreenPreviewWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IHierarchyNodeStringConverter, HierarchyNodeStringConverter>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScreenPaintForm, ScreenPaintForm>(new ContainerControlledLifetimeManager());
        }
    }
}
