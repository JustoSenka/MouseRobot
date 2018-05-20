using RobotRuntime.Graphics;
using RobotRuntime.Settings;

namespace RobotRuntime.Abstractions
{
    public interface IFeatureDetectorFactory
    {
        FeatureDetector Create(string Name);
        FeatureDetector GetFromCache(string Name);
    }
}