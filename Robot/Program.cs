﻿using Robot.Abstractions;
using Unity;
using Unity.Lifetime;
using Robot.Recording;
using Robot.Settings;
using Robot.Utils.Win32;

namespace Robot
{
    public static class Program
    {
        public static void RegisterInterfaces(UnityContainer Container)
        {
            Container.RegisterType<IAssetManager, AssetManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ICroppingManager, CroppingManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IMouseRobot, MouseRobot>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRecordingManager, RecordingManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScriptManager, ScriptManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISettingsManager, SettingsManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IInputCallbacks, InputCallbacks>(new ContainerControlledLifetimeManager());
        }
    }
}