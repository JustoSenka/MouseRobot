using System.Collections.Generic;
using System.Drawing;

namespace RobotRuntime.Graphics
{
    public abstract class TextDetector
    {
        public abstract string Name { get; }

        public abstract Point[] FindTextPosition(string text, Bitmap observedImage, float threshold = 0.80f);
        public abstract IEnumerable<Point[]> FindMultipleTextPositions(string text, Bitmap observedImage, float threshold = 0.80f);

        public virtual bool SupportsMultipleMatches { get { return false; } }

        public override string ToString() => Name;
    }
}
