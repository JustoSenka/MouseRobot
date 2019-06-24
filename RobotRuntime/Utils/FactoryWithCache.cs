using RobotRuntime.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity;
using Unity.Lifetime;

namespace RobotRuntime
{
    [RegisterTypeToContainer(typeof(IFactoryWithCache<>), typeof(ContainerControlledLifetimeManager))]
    public class FactoryWithCache<T> : IFactoryWithCache<T>
    {
        public IEnumerable<T> Detectors => TypeCollector.AllObjects;
        public IEnumerable<string> DetectorNames { get { return TypeCollector.AllObjects.Select(d => d.ToString()); } }

        private T m_DefaultDetector;
        public T DefaultDetector
        {
            get => m_DefaultDetector;
            set
            {
                m_DefaultDetector = value;
                m_DefaultDetectorName = m_DefaultDetector.ToString();
            }
        }

        private string m_DefaultDetectorName;

        private ILogger Logger;
        private IUnityContainer Container;
        private ITypeObjectCollector<T> TypeCollector;
        public FactoryWithCache(IUnityContainer Container, ILogger Logger, ITypeObjectCollector<T> TypeCollector)
        {
            this.Container = Container;
            this.Logger = Logger;
            this.TypeCollector = TypeCollector;
        }

        /// <summary>
        /// Not thread safe
        /// </summary>
        public T GetFromCache(string Name)
        {
            return FindDetectorOfName(Name);
        }

        /// <summary>
        /// Thread safe
        /// </summary>
        public T Create(string Name)
        {
            var detector = FindDetectorOfName(Name);
            try
            {
                return (T)Container.Resolve(detector.GetType());
            }
            catch (Exception e)
            {
                Logger.Logi(LogType.Error, "Cannot create image detector of name: " + Name, e.Message);
            }
            return default(T);
        }

        private T FindDetectorOfName(string Name)
        {
            if (Name.Equals(m_DefaultDetectorName, System.StringComparison.InvariantCultureIgnoreCase))
                return DefaultDetector;

            var detector = TypeCollector.AllObjects.FirstOrDefault(d => d.ToString().Equals(Name));

            if (detector == null)
            {
                Logger.Logi(LogType.Error, "Detector with name '" + Name + "' not found.", "Returning very first detector from the list.");
                detector = TypeCollector.AllObjects.ElementAt(0);
            }

            return detector;
        }
    }
}
