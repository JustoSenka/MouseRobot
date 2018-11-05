using Robot.Abstractions;
using Unity;
using Unity.Lifetime;
using Robot.Recording;
using Robot.Settings;
using Robot.Utils.Win32;
using Robot.Plugins;
using Robot.Scripts;
using Robot.Tests;
using Robot.Assets;

namespace Robot
{
    public static class Program
    {
        public static void RegisterInterfaces(UnityContainer Container)
        {
            //Container.RegisterInstance(typeof(IUnityContainer), Container, new ContainerControlledLifetimeManager());
            Container.RegisterType<IAssetManager, AssetManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ICroppingManager, CroppingManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IMouseRobot, MouseRobot>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRecordingManager, RecordingManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScriptManager, ScriptManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISettingsManager, SettingsManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IInputCallbacks, InputCallbacks>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IProjectManager, ProjectManager>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IPluginManager, PluginManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IPluginCompiler, PluginCompiler>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ICommandFactory, CommandFactory>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITestFixtureManager, TestFixtureManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITestRunnerManager, TestRunnerManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISolutionManager, SolutionManager>(new ContainerControlledLifetimeManager());

            // non singletons
            Container.RegisterType<IModifiedAssetCollector, ModifiedAssetCollector>();
        }
    }
}
