﻿using RobotRuntime.Utils;
using System;
using System.Drawing;

namespace Robot.Recording
{
    public class CroppingManager
    {
        public bool IsCropping { get; private set; }
        public Point StartPoint { get; private set; }

        public event Action<Point> ImageCropStarted;
        public event Action<Point> ImageCropEnded;
        public event Action ImageCropCanceled;
        public event Action<Bitmap> ImageCropped;

        public int LastCropImageIndex = 0;

        static private CroppingManager m_Instance = new CroppingManager();
        static public CroppingManager Instance { get { return m_Instance; } }
        private CroppingManager() { }

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
                if (AssetManager.Instance.GetAsset(GetPathForID(LastCropImageIndex)) != null)
                {
                    LastCropImageIndex++;
                    continue;
                }
                break;
            }

            AssetManager.Instance.CreateAsset(bmp, GetPathForID(LastCropImageIndex));
        }

        private static string GetPathForID(int imageIndex)
        {
            var length = 2 - imageIndex.ToString().Length;
            var num = ((length == 1) ? "0" : "") + imageIndex;
            return AssetManager.ImageFolder + "\\Crop_" + num + ".png";
        }
    }
}