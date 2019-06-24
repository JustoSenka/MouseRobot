using RobotRuntime.Graphics;
using System.Collections.Generic;
using System.Drawing;

namespace RobotRuntime
{
    public class TextDetectorCV : TextDetector
    {
        public override string Name => "Tesseract";

        public override IEnumerable<Point[]> FindImageMultiplePos(string sampleImage, string observedImage)
        {
            yield return new[] { new Point(100, 100), new Point(100, 105), new Point(105, 100), new Point(105, 105) };
        }

        public override Point[] FindImagePos(string sampleImage, string observedImage)
        {
            return new[] { new Point(100, 100), new Point(100, 105), new Point(105, 100), new Point(105, 105) };
        }
    }
}
