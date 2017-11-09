using RobotRuntime.Perf;
using RobotRuntime.Settings;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace RobotRuntime.Graphics
{
    public class FeatureDetectionThread : StableRepeatingThread
    {
        public Bitmap ObservedImage { get; private set; }
        public object ObservedImageLock = new object();

        private Bitmap m_SampleImage;
        private object m_SampleImageLock = new object();

        public Point[][] LastKnownPositions { get; private set; }
        public bool WasImageFound { get; private set; }
        public bool WasLastCheckSuccess { get; private set; }
        public int TimeSinceLastFind { get; private set; }

        public DetectionMode DetectionMode { get; set; } = DetectionMode.FeatureSURF;

        private Stopwatch m_Watch = new Stopwatch();

        private const float k_RectangleTolerance = 0.4f;

        public event Action<Point[][]> PositionFound;

        public static FeatureDetectionThread Instace { get { return m_Instance; } }
        private static FeatureDetectionThread m_Instance = new FeatureDetectionThread();
        private FeatureDetectionThread() { }

        protected override string Name { get { return "FeatureDetectionThread"; } }

        public override void Init()
        {
            base.Init();

            ObservedImage = new Bitmap(ScreenStateThread.Instace.Width, ScreenStateThread.Instace.Height, PixelFormat.Format32bppArgb);
            ScreenStateThread.Instace.Initialized += ScreenStateThreadInitialized;
        }

        private void ScreenStateThreadInitialized()
        {
            ObservedImage = new Bitmap(ScreenStateThread.Instace.Width, ScreenStateThread.Instace.Height, PixelFormat.Format32bppArgb);
        }

        protected override void ThreadAction()
        {
            if (ObservedImage == null)
                return;

            if (m_SampleImage == null)
                return;

            Profiler.Start(Name);

            Profiler.Start(Name + "_CloneScreen");
            lock (ScreenStateThread.Instace.ScreenBmpLock)
            {
                BitmapUtility.Clone32BPPBitmap(ScreenStateThread.Instace.ScreenBmp, ObservedImage);
            }
            Profiler.Stop(Name + "_CloneScreen");

            Profiler.Start(Name + "_FindMatch");
            var points = FindImagePositions();
            var success = ValidatePointsCorrectness(points);
            Profiler.Stop(Name + "_FindMatch");

            if (success)
            {
                PositionFound?.Invoke(points);
                LastKnownPositions = points;
                WasImageFound = true;
                WasLastCheckSuccess = true;
                TimeSinceLastFind = 0;
                m_Watch.Restart();
            }
            else
            {
                WasLastCheckSuccess = false;
                TimeSinceLastFind = (int)m_Watch.ElapsedMilliseconds;
            }

            Profiler.Stop(Name);
        }

        public void StartNewImageSearch(AssetPointer asset)
        {
            if (asset.Path.EndsWith(FileExtensions.Image))
            {
                lock (m_SampleImageLock)
                {
                    m_SampleImage = AssetImporter.FromPath(asset.Path).Load<Bitmap>();
                    WasLastCheckSuccess = false;
                    WasImageFound = false;
                    LastKnownPositions = null;
                    TimeSinceLastFind = 0;
                    m_Watch.Restart();
                }
            }
        }

        private Point[][] FindImagePositions()
        {
            lock (m_SampleImageLock)
            {
                if (FeatureDetector.Get(DetectionMode).SupportsMultipleMatches)
                    return FeatureDetector.Get(DetectionMode).FindImageMultiplePos(m_SampleImage, ObservedImage).ToArray();
                else
                    return new[] { FeatureDetector.Get(DetectionMode).FindImagePos(m_SampleImage, ObservedImage) };
            }
        }


        /// <summary>
        /// Returns false if points array is bad, or if it is not square like
        /// </summary>
        private bool ValidatePointsCorrectness(Point[][] points)
        {
            if (points == null || points.Length == 0 || points[0] == null || points[0].Length < 4)
                return false;

            if (points.GetLength(0) == 1 && points[0].Length == 4)
            {
                var threshold = k_RectangleTolerance * new Point(m_SampleImage.Width + m_SampleImage.Height).Magnitude() / 2;
                if (!points[0].IsRectangleish(threshold))
                    return false;
            }

            return true;
        }
    }
}
