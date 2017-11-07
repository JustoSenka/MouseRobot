using System.Drawing;

namespace RobotRuntime.Graphics
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

        public static Point[] ToPoint(this Rectangle rect)
        {
            return new Point[]
            {
                new Point(rect.Left, rect.Bottom),
                new Point(rect.Right, rect.Bottom),
                new Point(rect.Right, rect.Top),
                new Point(rect.Left, rect.Top)
            };
        }

        public static PointF[] ToPointF(this Rectangle rect)
        {
            return new PointF[]
            {
                new PointF(rect.Left, rect.Bottom),
                new PointF(rect.Right, rect.Bottom),
                new PointF(rect.Right, rect.Top),
                new PointF(rect.Left, rect.Top)
            };
        }

        public static PointF[] ToPoint(this RectangleF rect)
        {
            return new PointF[]
            {
                new PointF(rect.Left, rect.Bottom),
                new PointF(rect.Right, rect.Bottom),
                new PointF(rect.Right, rect.Top),
                new PointF(rect.Left, rect.Top)
            };
        }

        public static PointF[] ToPointF(this RectangleF rect)
        {
            return new PointF[]
            {
                new PointF(rect.Left, rect.Bottom),
                new PointF(rect.Right, rect.Bottom),
                new PointF(rect.Right, rect.Top),
                new PointF(rect.Left, rect.Top)
            };
        }

        public static Point[] ToPoint(this PointF[] points)
        {
            var size = points.Length;
            var newPoints = new Point[size];
            for (int i = 0; i < size; i++)
            {
                newPoints[i] = new Point((int)(points[i].X), (int)(points[i].Y));
            }
            return newPoints;
        }

        public static PointF[] ToPointF(this Point[] points)
        {
            var size = points.Length;
            var newPoints = new PointF[size];
            for (int i = 0; i < size; i++)
            {
                newPoints[i] = new PointF(points[i].X, points[i].Y);
            }
            return newPoints;
        }

        public static Point[] Scale(this Point[] points, float scale)
        {
            var size = points.Length;
            for (int i = 0; i < size; i++)
            {
                points[i] = new Point((int)(points[i].X * scale), (int)(points[i].Y * scale));
            }
            return points;
        }

        public static PointF[] Scale(this PointF[] points, float scale)
        {
            var size = points.Length;
            for (int i = 0; i < size; i++)
            {
                points[i] = new PointF(points[i].X * scale, points[i].Y * scale);
            }
            return points;
        }
    }
}
