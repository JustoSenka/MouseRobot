using RobotRuntime.Utils;
using System;
using System.Linq;
using RobotRuntime.Abstractions;
using System.Collections.Generic;
using Unity;

namespace RobotRuntime.Execution
{
    public class RunnerFactory : IRunnerFactory
    {
        private LightScript m_TestFixture;
        private CommandRunningCallback m_Callback;
        private ValueWrapper<bool> m_ShouldCancelRun;

        private Type[] m_NativeRunnerTypes;
        private Type[] m_UserRunnerTypes;
        private Type[] m_RunnerTypes;

        private IRunner[] m_NativeRunners;
        private IRunner[] m_UserRunners;
        private Dictionary<Type, IRunner> m_Runners;

        private IAssetGuidManager AssetGuidManager;
        private IFeatureDetectionThread FeatureDetectionThread;
        private ILogger Logger;
        private IPluginLoader PluginLoader;
        private IUnityContainer Container;
        public RunnerFactory(IUnityContainer Container, IPluginLoader PluginLoader, ILogger Logger,
            IFeatureDetectionThread FeatureDetectionThread, IAssetGuidManager AssetGuidManager)
        {
            this.Container = Container;
            this.PluginLoader = PluginLoader;
            this.Logger = Logger;
            this.AssetGuidManager = AssetGuidManager;
            this.FeatureDetectionThread = FeatureDetectionThread;

            PluginLoader.UserDomainReloaded += OnDomainReloaded;

            CollectNativeRunners();
            CollectUserRunners();
        }

        public void PassDependencies(LightScript TestFixture, CommandRunningCallback Callback, ValueWrapper<bool> ShouldCancelRun)
        {
            m_TestFixture = TestFixture;
            m_Callback = Callback;
            m_ShouldCancelRun = ShouldCancelRun;

            foreach (var pair in m_Runners)
                pair.Value.PassDependencies(this, TestFixture, Callback, ShouldCancelRun);
        }

        private void OnDomainReloaded()
        {
            CollectUserRunners();
        }

        private void CollectNativeRunners()
        {
            m_NativeRunnerTypes = AppDomain.CurrentDomain.GetNativeAssemblies().GetAllTypesWhichImplementInterface(typeof(IRunner)).ToArray();
            m_NativeRunners = m_NativeRunnerTypes.Select(t => Container.Resolve(t)).Cast<IRunner>().ToArray();

            foreach (var runner in m_NativeRunners)
                runner.PassDependencies(this, m_TestFixture, m_Callback, m_ShouldCancelRun);
        }

        private void CollectUserRunners()
        {
            // DO-DOMAIN: This will not work if assemblies are in different domain
            m_UserRunnerTypes = PluginLoader.IterateUserAssemblies(a => a).GetAllTypesWhichImplementInterface(typeof(IRunner)).ToArray();
            m_UserRunners = m_UserRunnerTypes.TryResolveTypes(Container, Logger).Cast<IRunner>().ToArray();

            foreach (var runner in m_UserRunners)
                runner.PassDependencies(this, m_TestFixture, m_Callback, m_ShouldCancelRun);

            m_RunnerTypes = m_NativeRunnerTypes.Concat(m_UserRunnerTypes).ToArray();

            m_Runners = new Dictionary<Type, IRunner>();
            foreach (var runner in m_NativeRunners.Concat(m_UserRunners))
                m_Runners.Add(runner.GetType(), runner);
        }

        public IRunner GetFor(Type commandType)
        {
            var runnerType = GetRunnerTypeForCommand(commandType);
            if (runnerType != null)
            {
                return m_Runners[runnerType];
            }
            else
            {
                Logger.Logi(LogType.Error, "Threre is no Runner registered that would support type: " + commandType);
                return m_Runners[typeof(SimpleCommandRunner)];
            }
        }

        public bool DoesRunnerSupportType(Type runnerType, Type commandType)
        {
            return GetRunnerTypeForCommand(commandType) != null;
        }

        private Type GetRunnerTypeForCommand(Type commandType)
        {
            var attribute = commandType.GetCustomAttributes(false).OfType<RunnerTypeAttribute>().FirstOrDefault();
            return attribute != null ? attribute.type : null;
        }
    }
}
