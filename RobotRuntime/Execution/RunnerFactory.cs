using System;
using System.Linq;
using RobotRuntime.Abstractions;
using System.Collections.Generic;
using Unity;
using RobotRuntime.Tests;

namespace RobotRuntime.Execution
{
    public class RunnerFactory : IRunnerFactory
    {
        private TestData m_TestData;

        private Type[] m_NativeRunnerTypes;
        private Type[] m_UserRunnerTypes;
        private Type[] m_RunnerTypes;

        private IRunner[] m_NativeRunners;
        private IRunner[] m_UserRunners;
        private Dictionary<Type, IRunner> m_Runners;

        private IAssetGuidManager AssetGuidManager;
        private IFeatureDetectionThread FeatureDetectionThread;
        private ILogger Logger;
        private IScriptLoader ScriptLoader;
        private IUnityContainer Container;
        public RunnerFactory(IUnityContainer Container, IScriptLoader ScriptLoader, ILogger Logger,
            IFeatureDetectionThread FeatureDetectionThread, IAssetGuidManager AssetGuidManager)
        {
            this.Container = Container;
            this.ScriptLoader = ScriptLoader;
            this.Logger = Logger;
            this.AssetGuidManager = AssetGuidManager;
            this.FeatureDetectionThread = FeatureDetectionThread;

            ScriptLoader.UserDomainReloaded += OnDomainReloaded;

            CollectNativeRunners();
            CollectUserRunners();
        }

        public void PassDependencies(TestData TestData)
        {
            this.m_TestData = TestData;
            TestData.RunnerFactory = this;
                
            foreach (var pair in m_Runners)
                pair.Value.TestData = TestData;
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
                runner.TestData = m_TestData;
        }

        private void CollectUserRunners()
        {
            // DO-DOMAIN: This will not work if assemblies are in different domain
            m_UserRunnerTypes = ScriptLoader.IterateUserAssemblies(a => a).GetAllTypesWhichImplementInterface(typeof(IRunner)).ToArray();
            m_UserRunners = m_UserRunnerTypes.TryResolveTypes(Container, Logger).Cast<IRunner>().ToArray();

            foreach (var runner in m_UserRunners)
                runner.TestData = m_TestData;

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
