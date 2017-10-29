using System.Drawing;

namespace Robot.Graphics
{
    public static class PointExtension
    {
        public static PointF Sub(this PointF p, PointF k)
        {
            return new PointF
            {
                X = p.X - k.X,
                Y = p.Y - k.Y
            };
        }

        public static PointF Sub(this PointF p, Point k)
        {
            return new PointF
            {
                X = p.X - k.X,
                Y = p.Y - k.Y
            };
        }

        public static PointF Add(this PointF p, PointF k)
        {
            return new PointF
            {
                X = p.X + k.X,
                Y = p.Y + k.Y
            };
        }

        public static PointF Add(this PointF p, Point k)
        {
            return new PointF
            {
                X = p.X + k.X,
                Y = p.Y + k.Y
            };
        }

        public static Point Sub(this Point p, PointF k)
        {
            return new Point
            {
                X = p.X - (int)k.X,
                Y = p.Y - (int)k.Y
            };
        }

        public static Point Sub(this Point p, Point k)
        {
            return new Point
            {
                X = p.X - k.X,
                Y = p.Y - k.Y
            };
        }

        public static Point Add(this Point p, PointF k)
        {
            return new Point
            {
                X = p.X + (int)k.X,
                Y = p.Y + (int)k.Y
            };
        }

        public static Point Add(this Point p, Point k)
        {
            return new Point
            {
                X = p.X + k.X,
                Y = p.Y + k.Y
            };
        }
    }
}
