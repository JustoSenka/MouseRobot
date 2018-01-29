using RobotRuntime.Graphics;
using RobotRuntime.Settings;

namespace RobotRuntime.Abstractions
{
    public interface IFeatureDetectorFactory
    {
        FeatureDetector Create(DetectionMode detectionMode);
        FeatureDetector GetFromCache(DetectionMode detectionMode);
    }
}