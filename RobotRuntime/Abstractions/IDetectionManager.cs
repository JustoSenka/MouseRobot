﻿using RobotRuntime.Settings;
using System.Drawing;
using System.Threading.Tasks;

namespace RobotRuntime.Abstractions
{
    public interface IDetectionManager
    {
        void ApplySettings(FeatureDetectionSettings settings);

        Task<Point[][]> FindImageRects(Bitmap sampleImage, string detectorName, int timeout);
        Task<Point[]> FindImage(Bitmap sampleImage, string detectorName, int timeout);
    }
}
