using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Unity.Lifetime;

namespace Robot.Graphics
{
    [RegisterTypeToContainer(typeof(IFeatureDetectionThread), typeof(ContainerControlledLifetimeManager))]
    public class FeatureDetectionThread : StableRepeatingThread, IFeatureDetectionThread
    {
        public Point[][] LastKnownPositions { get; private set; }
        public bool WasImageFound { get; private set; }
        public bool WasLastCheckSuccess { get; private set; }
        public int TimeSinceLastFind { get; private set; }

        public event Action<Point[][]> PositionFound;

        private Bitmap m_SampleImage;
        private Bitmap m_ObservedImage;
        private readonly object m_SampleImageLock = new object();

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

            m_ObservedImage = new Bitmap(ScreenStateThread.Width, ScreenStateThread.Height, PixelFormat.Format32bppArgb);
            ScreenStateThread.Initialized += ScreenStateThreadInitialized;
        }

        private void ScreenStateThreadInitialized()
        {
            m_ObservedImage = new Bitmap(ScreenStateThread.Width, ScreenStateThread.Height, PixelFormat.Format32bppArgb);
        }

        protected override void ThreadAction()
        {
            lock (ScreenStateThread.ScreenBmpLock)
            {
                BitmapUtility.Clone32BPPBitmap(ScreenStateThread.ScreenBmp, m_ObservedImage);
            }

            if (m_SampleImage == null || m_ObservedImage == null)
                return;

            var points = DetectionManager.FindImageRects(m_SampleImage, m_ObservedImage, m_DetectorName).Result;

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
