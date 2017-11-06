using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using RobotRuntime.Utils;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RobotRuntime.Graphics
{
    public abstract class FeatureDetector
    {
        protected abstract PointF[] FindImageLines(Mat sampleImage, Mat observedImage);
        public Point[] FindImageLines(Bitmap sampleImage, Bitmap observedImage, float scaleDownFactor = 1)
        {
            var o = observedImage.ToImage();
            var s = sampleImage.ToImage();
            var scale = scaleDownFactor;

            if (scale != 1)
            {
                o = o.Resize((int)(o.Width * scale), (int)(o.Height * scale), Inter.Linear);
                s = s.Resize((int)(s.Width * scale), (int)(s.Height * scale), Inter.Linear);
            }

            var points = FindImageLines(s.Mat, o.Mat);

            return points.Select(p => new Point((int)(p.X / scale), (int)(p.Y / scale))).ToArray();
        }

        protected abstract PointF[] FindImageRect(Mat sampleImage, Mat observedImage);
        public Point[] FindImageRect(Bitmap sampleImage, Bitmap observedImage, float scaleDownFactor = 1)
        {
            var o = observedImage.ToImage();
            var s = sampleImage.ToImage();
            var scale = scaleDownFactor;

            if (scale != 1)
            {
                o = o.Resize(scale, Inter.Linear);
                s = s.Resize(scale, Inter.Linear);
                
                list_o.Add(o);
                list_s.Add(s);
            }

            var points = FindImageRect(s.Mat, o.Mat);

            return points.Select(p => new Point((int)(p.X / scale), (int)(p.Y / scale))).ToArray();
        }

        private List<Image<Bgr, byte>> list_o = new List<Image<Bgr, byte>>();
        private List<Image<Bgr, byte>> list_s = new List<Image<Bgr, byte>>();

        private static FeatureDetector s_CurrentDetector;
        public static FeatureDetector Get()
        {
            if (s_CurrentDetector == null)
                s_CurrentDetector = new FeatureDetectorSURF();

            return s_CurrentDetector;
        }
    }
}
