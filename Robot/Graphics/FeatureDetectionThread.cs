using Robot.Abstractions;
using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using System;
using System.Diagnostics;
using System.Drawing;

namespace Robot.Graphics
{
    public class FeatureDetectionThread : StableRepeatingThread, IFeatureDetectionThread
    {
        public Bitmap ObservedImage { get; private set; }

        public object ObservedImageLock { get { return m_ObservedImageLock; } }
        private object m_ObservedImageLock = new object();

        private Bitmap m_SampleImage;
        private object m_SampleImageLock = new object();

        public Point[][] LastKnownPositions { get; private set; }
        public bool WasImageFound { get; private set; }
        public bool WasLastCheckSuccess { get; private set; }
        public int TimeSinceLastFind { get; private set; }

        public event Action<Point[][]> PositionFound;

        private string m_DetectorName;
        private Stopwatch m_Watch = new Stopwatch();

        private readonly IScreenStateThread ScreenStateThread;
        private readonly IProfiler Profiler;
        private readonly IFeatureDetectorFactory FeatureDetectorFactory;
        private readonly IDetectionManager DetectionManager;
        public FeatureDetectionThread(IScreenStateThread ScreenStateThread, IProfiler Profiler, IFeatureDetectorFactory FeatureDetectorFactory, IDetectionManager DetectionManager)
        {
            this.ScreenStateThread = ScreenStateThread;
            this.Profiler = Profiler;
            this.FeatureDetectorFactory = FeatureDetectorFactory;
            this.DetectionManager = DetectionManager;
        }

        protected override string Name { get { return "FeatureDetectionThread"; } }

        public override void Init()
        {
            base.Init();

            //ObservedImage = new Bitmap(ScreenStateThread.Width, ScreenStateThread.Height, PixelFormat.Format32bppArgb);
            ScreenStateThread.Initialized += ScreenStateThreadInitialized;
        }

        private void ScreenStateThreadInitialized()
        {
            //ObservedImage = new Bitmap(ScreenStateThread.Width, ScreenStateThread.Height, PixelFormat.Format32bppArgb);
        }

        protected override void ThreadAction()
        {
            /*
            lock (ScreenStateThread.ScreenBmpLock)
            {
                BitmapUtility.Clone32BPPBitmap(ScreenStateThread.ScreenBmp, ObservedImage);
            }*/

            if (m_SampleImage == null)
                return;

            var points = DetectionManager.FindImageRects(m_SampleImage, m_DetectorName, 1000).Result;

            if (points != null)
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
        }

        public void StartNewImageSearch(Bitmap sampleImage, string detector)
        {
            lock (m_SampleImageLock)
            {
                var clonedSampleImage = new Bitmap(sampleImage.Width, sampleImage.Height);
                m_SampleImage = BitmapUtility.Clone32BPPBitmap(sampleImage, clonedSampleImage);
                WasLastCheckSuccess = false;
                WasImageFound = false;
                LastKnownPositions = null;
                TimeSinceLastFind = 0;
                m_DetectorName = detector;
                m_Watch.Restart();
            }
        }
    }
}
