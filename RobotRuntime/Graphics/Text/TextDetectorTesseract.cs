using RobotRuntime.Graphics;
using RobotRuntime.Utils;
using System.Collections.Generic;
using System.Drawing;

namespace RobotRuntime
{
    public class TextDetectorTesseract : TextDetector
    {
        public override string Name => "Tesseract";
        public float Threshold => 0.80f;

        public override IEnumerable<Point[]> FindMultipleTextPositions(string text, Bitmap observedImage)
        {
            var rect = TesseractUtility.GetPositionOfTextFromImage(observedImage, text, new TesseractStringEqualityComparer(Threshold));
            yield return rect == default ? null : rect.ToPoint(); 
        }

        public override Point[] FindTextPosition(string text, Bitmap observedImage)
        {
            var rect = TesseractUtility.GetPositionOfTextFromImage(observedImage, text, new TesseractStringEqualityComparer(Threshold));
            return rect == default ? null : rect.ToPoint();
        }
    }
}
