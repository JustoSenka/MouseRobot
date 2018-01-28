using System;
using System.Drawing;

namespace Robot.Abstractions
{
    public interface ICroppingManager
    {
        bool IsCropping { get; }
        Point StartPoint { get; }

        event Action ImageCropCanceled;
        event Action<Point> ImageCropEnded;
        event Action<Bitmap> ImageCropped;
        event Action<Point> ImageCropStarted;

        void CancelCropImage();
        void EndCropImage(Point endPoint);
        void StartCropImage(Point startPoint);
    }
}