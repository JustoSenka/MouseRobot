﻿using System;
using System.Drawing;

namespace Robot.Abstractions
{
    public interface IFeatureDetectionThread : IStableRepeatingThread
    {
        object ObservedImageLock { get; }

        Point[][] LastKnownPositions { get; }
        Bitmap ObservedImage { get; }
        int TimeSinceLastFind { get; }
        bool WasImageFound { get; }
        bool WasLastCheckSuccess { get; }

        event Action<Point[][]> PositionFound;

        void StartNewImageSearch(Bitmap sampleImage, string detector);
    }
}