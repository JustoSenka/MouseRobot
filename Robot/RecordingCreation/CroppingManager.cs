using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Utils;
using System;
using System.Drawing;
using Unity.Lifetime;

namespace Robot.RecordingCreation
{
    [RegisterTypeToContainer(typeof(ICroppingManager), typeof(ContainerControlledLifetimeManager))]
    public class CroppingManager : ICroppingManager
    {
        public bool IsCropping { get; private set; }
        public Point StartPoint { get; private set; }

        public event Action<Point> ImageCropStarted;
        public event Action<Point> ImageCropEnded;
        public event Action ImageCropCanceled;
        public event Action<Bitmap> ImageCropped;

        public int LastCropImageIndex = 0;

        private IAssetManager AssetManager;
        public CroppingManager(IAssetManager AssetManager)
        {
            this.AssetManager = AssetManager;
        }

        public void StartCropImage(Point startPoint)
        {
            IsCropping = true;
            StartPoint = startPoint;
            ImageCropStarted?.Invoke(StartPoint);
        }

        public void EndCropImage(Point endPoint)
        {
            var r = BitmapUtility.GetRect(StartPoint, endPoint);
            if (r.Width < 10 || r.Height < 10)
                return;

            var bmp = BitmapUtility.TakeScreenshotOfSpecificRect(StartPoint, endPoint);

            CreateNewAsset(bmp);

            ImageCropped?.Invoke(bmp);
            ImageCropEnded?.Invoke(endPoint);

            IsCropping = false;
            StartPoint = Point.Empty;
        }

        public void CancelCropImage()
        {
            ImageCropCanceled?.Invoke();
            ImageCropEnded?.Invoke(Point.Empty);

            IsCropping = false;
            StartPoint = Point.Empty;
        }

        private void CreateNewAsset(Bitmap bmp)
        {
            while (true)
            {
                if (AssetManager.GetAsset(GetPathForID(LastCropImageIndex)) != null)
                {
                    LastCropImageIndex++;
                    continue;
                }
                break;
            }

            AssetManager.CreateAsset(bmp, GetPathForID(LastCropImageIndex));
        }

        private string GetPathForID(int imageIndex)
        {
            var length = 2 - imageIndex.ToString().Length;
            var num = ((length == 1) ? "0" : "") + imageIndex;
            return Paths.AssetsPath + "\\Crop_" + num + ".png";
        }
    }
}
