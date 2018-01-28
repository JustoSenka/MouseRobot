using System;
using System.Drawing;

namespace Robot.Abstractions
{
    public interface IRecordingManager
    {
        bool IsRecording { get; set; }

        event Action<Asset, Point> ImageFoundInAssets;
        event Action<Point> ImageNotFoundInAssets;
    }
}