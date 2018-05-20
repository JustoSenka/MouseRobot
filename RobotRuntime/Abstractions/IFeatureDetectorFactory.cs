using RobotRuntime.Graphics;
using RobotRuntime.Settings;
using System.Collections.Generic;

namespace RobotRuntime.Abstractions
{
    public interface IFeatureDetectorFactory
    {
        IEnumerable<FeatureDetector> Detectors { get; }
        IEnumerable<string> DetectorNames { get; }

        FeatureDetector Create(string Name);
        FeatureDetector GetFromCache(string Name);
    }
}