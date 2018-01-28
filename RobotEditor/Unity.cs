using RobotRuntime.Abstractions;
using RobotEditor.Abstractions;
using Robot.Abstractions;
using RobotRuntime.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Container;
using Unity.Lifetime;
using RobotRuntime.Graphics;
using RobotRuntime.Settings;
using Robot;
using Robot.Recording;
using Robot.Settings;
using RobotEditor;
using RobotEditor.Windows;
using RobotRuntime.Execution;

namespace RobotRuntime
{
    public static class Unity
    {
        public static UnityContainer Container;

        public static void Init()
        {
            Container = new UnityContainer();

            Container.RegisterType<IMainForm, MainForm>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IAssetGuidManager, AssetGuidManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IFeatureDetectionThread, FeatureDetectionThread>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRuntimeSettings, RuntimeSettings>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScreenStateThread, ScreenStateThread>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITestRunner, TestRunner>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRunnerFactory, RunnerFactory>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IAssetManager, AssetManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ICroppingManager, CroppingManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IMouseRobot, MouseRobot>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRecordingManager, RecordingManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScriptManager, ScriptManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISettingsManager, SettingsManager>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IAssetsWindow, AssetsWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IHierarchyWindow, HierarchyWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IInspectorWindow, InspectorWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IProfilerWindow, ProfilerWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IPropertiesWindow, PropertiesWindow>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScreenDrawForm, ScreenDrawForm>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScreenPreviewWindow, ScreenPreviewWindow>(new ContainerControlledLifetimeManager());
        }
    }
}
