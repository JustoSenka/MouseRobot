using RobotRuntime.Abstractions;
using RobotRuntime.Settings;

namespace RobotRuntime.Graphics
{
    public class FeatureDetectorFactory : IFeatureDetectorFactory
    {
        private FeatureDetector s_CurrentDetector;
        private DetectionMode s_CurrentDetectionMode;
        /// <summary>
        /// Not Thread Safe
        /// </summary>
        public FeatureDetector GetFromCache(DetectionMode detectionMode)
        {
            if (s_CurrentDetector != null && detectionMode == s_CurrentDetectionMode)
                return s_CurrentDetector;
            else
            {
                s_CurrentDetectionMode = detectionMode;
                return s_CurrentDetector = Create(detectionMode);
            }
        }

        /// <summary>
        /// Thread Sade but creates garbage
        /// </summary>
        public FeatureDetector Create(DetectionMode detectionMode)
        {
            switch (detectionMode)
            {
                case DetectionMode.FeatureSURF:
                    return new FeatureDetectorSURF();
                case DetectionMode.PixelPerfect:
                    return new FeatureDetectorPP();
                case DetectionMode.Template:
                    return new FeatureDetectorTemplate();
                default:
                    return null;
            }
        }
    }
}
