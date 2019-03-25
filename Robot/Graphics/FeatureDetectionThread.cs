using Robot.Abstractions;
using RobotRuntime.Abstractions;
using RobotRuntime.Graphics;
using RobotRuntime.Settings;
using RobotRuntime.Utils;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

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

        // TODO: This is very vulnerable public field. User can change it and everything would break.
        // Find a better way to pass settings to this class
        /// <summary>
        /// This detector is set from SettingsManager or RuntimeSettings
        /// </summary>
        public string DefaultDetectorName { get; set; } = DetectorNamesHardcoded.Default; 

        /// <summary>
        /// This detector is set from commands when they want to override default detector.
        /// </summary>
        public string DetectorName { get; set; } = DetectorNamesHardcoded.Default;

        /// <summary>
        /// Return detector from Settings, if user did not override it with anything else than default.
        /// </summary>
        public string PrefferedDetector => DetectorName.Equals(DetectorNamesHardcoded.Default, StringComparison.InvariantCultureIgnoreCase) ? DefaultDetectorName : DetectorName;

        private Stopwatch m_Watch = new Stopwatch();

        private const float k_RectangleTolerance = 0.4f;

        public event Action<Point[][]> PositionFound;

        private IScreenStateThread ScreenStateThread;
        private IProfiler Profiler;
        private IFeatureDetectorFactory FeatureDetectorFactory;
        public FeatureDetectionThread(IScreenStateThread ScreenStateThread, IProfiler Profiler, IFeatureDetectorFactory FeatureDetectorFactory)
        {
            this.ScreenStateThread = ScreenStateThread;
            this.Profiler = Profiler;
            this.FeatureDetectorFactory = FeatureDetectorFactory;
        }

        protected override string Name { get { return "FeatureDetectionThread"; } }

        public override void Init()
        {
            base.Init();

            ObservedImage = new Bitmap(ScreenStateThread.Width, ScreenStateThread.Height, PixelFormat.Format32bppArgb);
            ScreenStateThread.Initialized += ScreenStateThreadInitialized;
        }

        private void ScreenStateThreadInitialized()
        {
            ObservedImage = new Bitmap(ScreenStateThread.Width, ScreenStateThread.Height, PixelFormat.Format32bppArgb);
        }

        protected override void ThreadAction()
        {
            if (ObservedImage == null)
                return;

            if (m_SampleImage == null)
                return;

            Profiler.Start(Name);

            Profiler.Start(Name + "_CloneScreen");
            lock (ScreenStateThread.ScreenBmpLock)
            {
                BitmapUtility.Clone32BPPBitmap(ScreenStateThread.ScreenBmp, ObservedImage);
            }
            Profiler.Stop(Name + "_CloneScreen");

            Profiler.Start(Name + "_FindImage");
            var points = FindImagePositions(PrefferedDetector);
            var success = ValidatePointsCorrectness(points);
            Profiler.Stop(Name + "_FindImage");

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
                DetectorName = detector;
                m_Watch.Restart();
            }
        }

        /// <summary>
        /// Returns center points of all found images
        /// </summary>
        public Point[] FindImageSync(Bitmap sampleImage, string detector, int timeout)
        {
            StartNewImageSearch(sampleImage, detector);
            while (timeout > TimeSinceLastFind)
            {
                Task.Delay(5).Wait(); // It will probably wait 15-30 ms, depending on thread clock, find better solution
                if (WasImageFound)
                    return LastKnownPositions.Select(p => p.FindCenter()).ToArray();
            }

            return null;
        }

        private Point[][] FindImagePositions(string detectorName)
        {
            var detector = FeatureDetectorFactory.GetFromCache(detectorName);

            lock (m_SampleImageLock)
            {
                if (detector.SupportsMultipleMatches)
                    return detector.FindImageMultiplePos(m_SampleImage, ObservedImage).ToArray();
                else
                    return new[] { detector.FindImagePos(m_SampleImage, ObservedImage) };
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
