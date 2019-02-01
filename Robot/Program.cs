using Robot.Abstractions;
using Unity;
using Unity.Lifetime;
using Robot.RecordingCreation;
using Robot.Settings;
using Robot.Utils.Win32;
using Robot.Scripts;
using Robot.Recordings;
using Robot.Tests;
using Robot.Assets;
using Robot.Utils;
using RobotRuntime.Abstractions;

namespace Robot
{
    public static class Program
    {
        public static void RegisterInterfaces(UnityContainer Container)
        {
            //Container.RegisterInstance(typeof(IUnityContainer), Container, new ContainerControlledLifetimeManager());
            Container.RegisterType<IAssetManager, AssetManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRuntimeAssetManager, AssetManager>(new ContainerControlledLifetimeManager());

            Container.RegisterType<ICroppingManager, CroppingManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IMouseRobot, MouseRobot>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRecordingManager, RecordingManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IHierarchyManager, HierarchyManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISettingsManager, SettingsManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IInputCallbacks, InputCallbacks>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IProjectManager, ProjectManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRuntimeProjectManager, ProjectManager>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IScriptManager, ScriptManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScriptCompiler, ScriptCompiler>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ICommandFactory, CommandFactory>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITestFixtureManager, TestFixtureManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITestRunnerManager, TestRunnerManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISolutionManager, SolutionManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ICodeEditor, CodeEditorVS>(new ContainerControlledLifetimeManager());

            // non singletons
            Container.RegisterType<IModifiedAssetCollector, ModifiedAssetCollector>();
        }
    }
}
