using RobotRuntime.Graphics;
using System.Collections.Generic;
using System.Drawing;

namespace CustomNamespace
{
    public class BigImageDetectorFake : FeatureDetector
    {
        public override string Name { get { return "FakeDetector"; } }
        public override bool SupportsMultipleMatches { get { return true; } }

        public override IEnumerable<Point[]> FindImageMultiplePos(Bitmap smallBmp, Bitmap bigBmp)
        {
            yield return smallBmp.Width >= 10 ? MakeARect(new Point(smallBmp.Width, smallBmp.Height)) : null;
        }

        public override Point[] FindImagePos(Bitmap smallBmp, Bitmap bigBmp)
        {
            return smallBmp.Width >= 10 ? MakeARect(new Point(smallBmp.Width, smallBmp.Height)) : null;
        }

        public Point[] MakeARect(Point p)
        {
            return new[] { p, p, p, p };
        }
    }
}
