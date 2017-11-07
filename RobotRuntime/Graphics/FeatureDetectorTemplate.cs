using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Collections.Generic;
using RobotRuntime.Utils;

namespace RobotRuntime.Graphics
{
    public class FeatureDetectorTemplate : FeatureDetector
    {
        private const float Threshold = 0.9f;

        public override IEnumerable<Point[]> FindImageMultiplePos(Bitmap sampleImage, Bitmap observedImage)
        {
            var model = sampleImage.ToImage();
            var observed = observedImage.ToImage();
            // TODO: Should I use grayscale images here?

            Point[] points = null;
            using (Image<Gray, float> result = observed.MatchTemplate(model, TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;
                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                if (maxValues[0] > Threshold) // Currently finds only one, not sure why
                {
                    points = new Rectangle(maxLocations[0], model.Size).ToPoint();
                }
            }
            yield return points;
        }

        public override Point[] FindImagePos(Bitmap sampleImage, Bitmap observedImage)
        {
            var model = sampleImage.ToImage();
            var observed = observedImage.ToImage();

            var scale = SmartResize(ref observed, ref model);

            Point[] points = null;
            using (Image<Gray, float> result = observed.MatchTemplate(model, TemplateMatchingType.CcoeffNormed))
            {
                double[] minValues, maxValues;
                Point[] minLocations, maxLocations;
                result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                if (maxValues[0] > Threshold)
                {
                    points = new Rectangle(maxLocations[0], model.Size).ToPoint();
                }
            }
            return points?.Scale(1 / scale);
        }
    }
}
