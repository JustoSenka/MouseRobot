using RobotRuntime.Graphics;
using System.Collections.Generic;
using System.Drawing;

namespace RobotEditor.Resources.ScriptTemplates
{
    public class CustomFeatureDetector : FeatureDetector
    {
        public override string Name { get { return "Custom Detector"; } } // Name must be unique. It is used in settings to choose detector

        public override bool SupportsMultipleMatches { get { return true; } } // If detector cannot match multiple images on screen, set this to false

        // Sample image is usually a screen or application image, while observed image is small one to look for
        public override IEnumerable<Point[]> FindImageMultiplePos(Bitmap sampleImage, Bitmap observedImage)
        {
            yield return new[] { new Point(50, 100), new Point(100, 100), new Point(100, 50), new Point(50, 50) };
        }

        public override Point[] FindImagePos(Bitmap sampleImage, Bitmap observedImage)
        {
            return new[] { new Point(50, 100), new Point(100, 100), new Point(100, 50), new Point(50, 50) };
        }
    }
}
