using System.Collections.Generic;

namespace RobotRuntime.Abstractions
{
    public interface IFactoryWithCache<T>
    {
        T DefaultDetector { get; set; }
        IEnumerable<string> DetectorNames { get; }
        IEnumerable<T> Detectors { get; }

        T Create(string Name);
        T GetFromCache(string Name);
    }
}
