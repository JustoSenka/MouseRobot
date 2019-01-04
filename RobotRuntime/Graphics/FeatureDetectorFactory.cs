using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace RobotRuntime.Graphics
{
    public class FeatureDetectorFactory : IFeatureDetectorFactory
    {
        public IEnumerable<FeatureDetector> Detectors { get { return m_Detectors; } }
        public IEnumerable<string> DetectorNames { get { return m_DetectorNames; } }

        private Type[] m_NativeDetectorTypes;
        private Type[] m_UserDetectorTypes;
        private Type[] m_DetectorTypes;

        private FeatureDetector[] m_NativeDetectors;
        private FeatureDetector[] m_UserDetectors;
        private FeatureDetector[] m_Detectors;

        private string[] m_DetectorNames;

        private ILogger Logger;
        private IScriptLoader PluginLoader;
        private IUnityContainer Container;
        public FeatureDetectorFactory(IUnityContainer Container, IScriptLoader PluginLoader, ILogger Logger)
        {
            this.Container = Container;
            this.PluginLoader = PluginLoader;
            this.Logger = Logger;

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
            m_NativeDetectorTypes = AppDomain.CurrentDomain.GetNativeAssemblies().GetAllTypesWhichImplementInterface(typeof(FeatureDetector)).ToArray();
            m_NativeDetectors = m_NativeDetectorTypes.Select(t => Container.Resolve(t)).Cast<FeatureDetector>().ToArray();
        }

        private void CollectUserDetectors()
        {
            // DO-DOMAIN: This will not work if assemblies are in different domain
            m_UserDetectorTypes = PluginLoader.IterateUserAssemblies(a => a).GetAllTypesWhichImplementInterface(typeof(FeatureDetector)).ToArray();
            m_UserDetectors = m_UserDetectorTypes.TryResolveTypes(Container, Logger).Cast<FeatureDetector>().ToArray();

            m_DetectorTypes = m_NativeDetectorTypes.Concat(m_UserDetectorTypes).ToArray();
            m_Detectors = m_NativeDetectors.Concat(m_UserDetectors).ToArray();
            m_DetectorNames = m_Detectors.Select(d => d.Name).ToArray();
        }

        /// <summary>
        /// Not thread safe
        /// </summary>
        public FeatureDetector GetFromCache(string Name)
        {
            return FindDetectorOfName(Name);
        }

        /// <summary>
        /// Thread safe
        /// </summary>
        public FeatureDetector Create(string Name)
        {
            var detector = FindDetectorOfName(Name);
            return (FeatureDetector) Container.Resolve(detector.GetType());
        }

        private FeatureDetector FindDetectorOfName(string Name)
        {
            var detector = m_Detectors.FirstOrDefault(d => d.Name.Equals(Name));

            if (detector == null)
            {
                Logger.Logi(LogType.Error, "Detector with name '" + Name + "' not found.", "Returning very first detector from the list.");
                detector = m_Detectors[0];
            }

            return detector;
        }
    }
}
