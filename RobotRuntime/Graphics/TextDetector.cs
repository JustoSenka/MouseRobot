using System.Collections.Generic;
using System.Drawing;

namespace RobotRuntime.Graphics
{
    public abstract class TextDetector
    {
        public abstract string Name { get; }

        public abstract Point[] FindImagePos(string sampleImage, string observedImage);
        public abstract IEnumerable<Point[]> FindImageMultiplePos(string sampleImage, string observedImage);

        public virtual bool SupportsMultipleMatches { get { return false; } }

        public override string ToString() => Name;
    }
}
