using Emgu.CV;
using RobotRuntime.Utils;
using System;
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

            
            lock (ScreenStateThread.Instace.ScreenBmpLock)
            {
                BitmapUtility.Clone32BPPBitmap(ScreenStateThread.Instace.ScreenBmp, ObservedImage);
            }
            /*
            PointF[] points;
            lock (ObservedImageLock)
            {
                ObservedImage = BitmapUtility.TakeScreenshot();
                
                long time;
                points = FeatureDetection.FindImagePos(m_SampleMat, ObservedImage.ToMat(), out time);
            }*/

            long time;
            var points = FeatureDetection.FindImagePos(m_SampleMat, ObservedImage.ToMat(), out time);

            PositionFound?.Invoke(points.Select(p => new Point((int)p.X, (int)p.Y)).ToArray());
        }

        public AssetPointer SampleImageFromAsset
        {
            set
            {
                lock (m_SampleMatLock)
                    m_SampleMat = new Mat(value.Path);
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
