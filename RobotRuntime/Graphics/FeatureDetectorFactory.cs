using RobotRuntime.Abstractions;
using RobotRuntime.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace RobotRuntime.Graphics
{
    public class FeatureDetectorFactory : IFeatureDetectorFactory
    {
        public IEnumerable<FeatureDetector> Detectors => TypeCollector.AllObjects;
        public IEnumerable<string> DetectorNames { get { return TypeCollector.AllObjects.Select(d => d.Name); } }

        private FeatureDetector DefaultDetector;
        private readonly string m_DefaultDetectorName = DetectorNamesHardcoded.Default;

        private ILogger Logger;
        private IUnityContainer Container;
        private ITypeObjectCollector<FeatureDetector> TypeCollector;
        public FeatureDetectorFactory(IUnityContainer Container, ILogger Logger, ITypeObjectCollector<FeatureDetector> TypeCollector)
        {
            this.Container = Container;
            this.Logger = Logger;
            this.TypeCollector = TypeCollector;

            DefaultDetector = new FeatureDetectorPP();
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
            try
            {
                return (FeatureDetector)Container.Resolve(detector.GetType());
            }
            catch (Exception e)
            {
                Logger.Logi(LogType.Error,"Cannot create image detector of name: " + Name, e.Message);
            }
            return null;
        }

        private FeatureDetector FindDetectorOfName(string Name)
        {
            if (Name.Equals(m_DefaultDetectorName, System.StringComparison.InvariantCultureIgnoreCase))
                return DefaultDetector;

            var detector = TypeCollector.AllObjects.FirstOrDefault(d => d.Name.Equals(Name));

            if (detector == null)
            {
                Logger.Logi(LogType.Error, "Detector with name '" + Name + "' not found.", "Returning very first detector from the list.");
                detector = TypeCollector.AllObjects.ElementAt(0);
            }

            return detector;
        }
    }
}
