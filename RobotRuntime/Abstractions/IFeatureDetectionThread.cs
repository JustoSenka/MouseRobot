using System;
using System.Drawing;

namespace RobotRuntime.Abstractions
{
    public interface IFeatureDetectionThread : IStableRepeatingThread
    {
        object ObservedImageLock { get; }

        string DetectorName { get; set; }

        Point[][] LastKnownPositions { get; }
        Bitmap ObservedImage { get; }
        int TimeSinceLastFind { get; }
        bool WasImageFound { get; }
        bool WasLastCheckSuccess { get; }

        event Action<Point[][]> PositionFound;

        void StartNewImageSearch(Bitmap sampleImage);
        Point[] FindImageSync(Bitmap sampleImage, int timeout);
    }
}