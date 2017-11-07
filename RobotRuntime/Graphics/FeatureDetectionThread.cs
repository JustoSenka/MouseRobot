using RobotRuntime.Perf;
using RobotRuntime.Utils;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace RobotRuntime.Graphics
{
    public class FeatureDetectionThread : StableRepeatingThread
    {
        public Point ImagePos { get; private set; }

        public Bitmap ObservedImage { get; private set; }
        public object ObservedImageLock = new object();

        public Bitmap m_SampleImage;
        private object m_SampleImageLock = new object();

        public event Action<Point[]> PositionFound;

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
            Point[] points;
            lock (m_SampleImageLock)
            {
                points = FeatureDetector.Get().FindImagePos(m_SampleImage, ObservedImage);
            }
            Profiler.Stop(Name + "_FindMatch");

            PositionFound?.Invoke(points);
            Profiler.Stop(Name);
        }

        public AssetPointer SampleImageFromAsset
        {
            set
            {
                lock (m_SampleImageLock)
                {             
                    if (value.Path.EndsWith(FileExtensions.Image))
                        m_SampleImage = AssetImporter.FromPath(value.Path).Load<Bitmap>();
                }
            }
        }

        public Bitmap SampleImage
        {
            set
            {
                lock (m_SampleImageLock)
                {
                    m_SampleImage = value;
                }
            }
        }
    }
}
