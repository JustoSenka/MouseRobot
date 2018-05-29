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
        public IEnumerable<IRunner> Runners { get { return m_Runners; } }

        public CommandRunningCallback Callback { get; set; }
        public LightScript ExecutingScript { get; set; }
        public ValueWrapper<bool> CancellingPointerPlaceholder { get; set; }

        private Type[] m_NativeRunnerTypes;
        private Type[] m_UserRunnerTypes;
        private Type[] m_RunnerTypes;

        private IRunner[] m_NativeRunners;
        private IRunner[] m_UserRunners;
        private IRunner[] m_Runners;

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

            CollectNativeDetectors();
            CollectUserDetectors();
        }

        private void OnDomainReloaded()
        {
            CollectUserDetectors();
        }

        private void CollectNativeDetectors()
        {
            m_NativeRunnerTypes = AppDomain.CurrentDomain.GetNativeAssemblies().GetAllTypesWhichImplementInterface(typeof(IRunner)).ToArray();
            // m_NativeRunners = m_NativeRunnerTypes.Select(t => Container.Resolve(t)).Cast<IRunner>().ToArray();
            m_NativeRunners = m_NativeRunnerTypes.Select(t => CreateFor(t)).Cast<IRunner>().ToArray();
        }

        private void CollectUserDetectors()
        {
            // DO-DOMAIN: This will not work if assemblies are in different domain
            m_UserRunnerTypes = PluginLoader.IterateUserAssemblies(a => a).GetAllTypesWhichImplementInterface(typeof(IRunner)).ToArray();
            m_UserRunners = m_UserRunnerTypes.TryResolveTypes(Container, Logger).Cast<IRunner>().ToArray();

            m_RunnerTypes = m_NativeRunnerTypes.Concat(m_UserRunnerTypes).ToArray();
            m_Runners = m_NativeRunners.Concat(m_UserRunners).ToArray();
        }

        public IRunner CreateFor(Type type)
        {
            if (DoesRunnerSupportType(typeof(SimpleCommandRunner), type))
                return new SimpleCommandRunner(this, Callback);

            else if (DoesRunnerSupportType(typeof(ImageCommandRunner), type))
                return new ImageCommandRunner(this, FeatureDetectionThread, AssetGuidManager, ExecutingScript, Callback, CancellingPointerPlaceholder);

            else if (DoesRunnerSupportType(typeof(ScriptRunner), type))
                return new ScriptRunner(this, Callback, CancellingPointerPlaceholder);

            else
            {
                Logger.Logi(LogType.Error, "Threre is no Runner registered that would support type: " + type);
                return new SimpleCommandRunner(this, Callback);
            }
        }

        // TODO: currently runner is created for every single command, and this is executed quite often, might be slow. Consider caching everyhing in Dictionary
        public bool DoesRunnerSupportType(Type runnerType, Type supportedType)
        {
            return runnerType.GetCustomAttributes(false).OfType<SupportedTypeAttribute>().Where(a => a.type == supportedType).Count() > 0;
        }
    }
}
