using RobotRuntime.Abstractions;
using RobotRuntime.Settings;
using RobotRuntime.Utils;
using System.Drawing;
using System.Linq;
using Unity.Lifetime;

namespace RobotRuntime.Graphics
{
    [RegisterTypeToContainer(typeof(IFeatureDetectionManager), typeof(ContainerControlledLifetimeManager))]
    public class FeatureDetectionManager : BaseDetectionManager, IFeatureDetectionManager
    {
        private Bitmap m_CachedClonedSampleImage;
        private FeatureDetector m_FeatureDetector;

        private readonly IProfiler Profiler;
        private readonly IFactoryWithCache<FeatureDetector> FeatureDetectorFactory;
        public FeatureDetectionManager(IProfiler Profiler, IFactoryWithCache<FeatureDetector> FeatureDetectorFactory)
        {
            this.Profiler = Profiler;
            this.FeatureDetectorFactory = FeatureDetectorFactory;
        }

        public override void ApplySettings(DetectionSettings settings)
        {
            DefaultDetector = settings.ImageDetectionMode;
            FeatureDetectorFactory.DefaultInstanceName = settings.ImageDetectionMode;
        }

        /// <summary>
        /// Used to do preparations such as detectable caching for images
        /// </summary>
        protected override bool BeforeStartingSearch(Detectable detectable, string detectorName, int timeout, Bitmap observedImage = null)
        {
            if (!(detectable.Value is Bitmap sampleImage))
                return false;

            // Clone and Cache image for faster use in the loop for search
            m_CachedClonedSampleImage = null;
            lock (sampleImage)
            {
                m_CachedClonedSampleImage = BitmapUtility.Clone32BPPBitmap(sampleImage, new Bitmap(sampleImage.Width, sampleImage.Height));
            }

            if (m_CachedClonedSampleImage == null)
            {
                Logger.Log(LogType.Error, "Cloned sample image was null. That's a bug, please report it.");
                return false;
            }

            // Create or get cached detector. This method is called from base class, so cannot use specific detector type
            m_FeatureDetector = FeatureDetectorFactory.Create(GetPreferredDetector(detectorName));
            if (m_FeatureDetector == null)
                return false;

            return true;
        }

        /// <summary>
        /// Returns array of positions for given image and detector.
        /// Always returns array even if Feature Detector supports only single images
        /// </summary>
        protected override bool FindRectsSync(Detectable detectable, string detectorName, Bitmap observedImage, out Point[][] points)
        {
            // Return array of points no matter which mode it supports so other systems can rely on only one entry point of API
            if (m_FeatureDetector.SupportsMultipleMatches)
                points = m_FeatureDetector.FindImageMultiplePos(m_CachedClonedSampleImage, observedImage).ToArray();
            else
                points = new[] { m_FeatureDetector.FindImagePos(m_CachedClonedSampleImage, observedImage) };

            return ValidatePointsCorrectness(points, m_CachedClonedSampleImage.Width, m_CachedClonedSampleImage.Height);
        }
    }
}
