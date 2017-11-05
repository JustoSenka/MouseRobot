using Emgu.CV;
using RobotRuntime.Perf;
using RobotRuntime.Utils;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace RobotRuntime.Graphics
{
    public class FeatureDetectionThread : StableRepeatingThread
    {
        public Point ImagePos { get; private set; }

        public Bitmap ObservedImage { get; private set; }
        public object ObservedImageLock = new object();

        private Mat m_SampleMat;
        private object m_SampleMatLock = new object();

        public static FeatureDetectionThread Instace { get { return m_Instance; } }
        private static FeatureDetectionThread m_Instance = new FeatureDetectionThread();
        private FeatureDetectionThread() { }

        public event Action<Point[]> PositionFound;

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

            if (m_SampleMat == null)
                return;

            Profiler.Start(Name);

            Profiler.Start(Name + "_CloneScreen");
            lock (ScreenStateThread.Instace.ScreenBmpLock)
            {
                BitmapUtility.Clone32BPPBitmap(ScreenStateThread.Instace.ScreenBmp, ObservedImage);
            }
            Profiler.Stop(Name + "_CloneScreen");

            Profiler.Start(Name + "_FindMatch");
            long time;
            var points = FeatureDetection.FindImagePos(m_SampleMat, ObservedImage.ToMat(), out time);
            Profiler.Stop(Name + "_FindMatch");


            PositionFound?.Invoke(points.Select(p => new Point((int)p.X, (int)p.Y)).ToArray());
            Profiler.Stop(Name);
        }

        public AssetPointer SampleImageFromAsset
        {
            set
            {
                lock (m_SampleMatLock)
                {
                    if (value.Path.EndsWith(FileExtensions.Image))
                        m_SampleMat = new Mat(value.Path);
                }
            }
        }

        public Bitmap SampleImageFromBitmap
        {
            set
            {
                lock (m_SampleMatLock)
                    m_SampleMat = value.ToMat();
            }
        }
    }
}
