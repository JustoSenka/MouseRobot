using RobotRuntime.Graphics;
using RobotRuntime.Utils;
using System.Collections.Generic;
using System.Drawing;

namespace RobotRuntime
{
    public class TextDetectorTesseract : TextDetector
    {
        public override string Name => "Tesseract";

        public override IEnumerable<Point[]> FindMultipleTextPositions(string text, Bitmap observedImage, float threshold = 0.80f)
        {
            var rect = TesseractUtility.GetPositionOfTextFromImage(observedImage, text, new TesseractStringEqualityComparer(threshold));
            yield return rect == default ? null : rect.ToPoint(); 
        }

        public override Point[] FindTextPosition(string text, Bitmap observedImage, float threshold = 0.80f)
        {
            var rect = TesseractUtility.GetPositionOfTextFromImage(observedImage, text, new TesseractStringEqualityComparer(threshold));
            return rect == default ? null : rect.ToPoint();
        }
    }
}
