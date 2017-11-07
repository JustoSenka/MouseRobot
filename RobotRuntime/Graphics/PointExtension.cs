using System;
using System.Drawing;
using System.Linq;

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

        /// <summary>
        /// Returns false if points are not persistent in distance from rectangle center.
        /// threshold - distance difference from center which is still allowed
        /// </summary>
        public static bool IsRectangleish(this Point[] p, float threshold)
        {
            float dd1, dd2, dd3, dd4;
            threshold = sqr(threshold);

            var c = p.FindCenter();
            float cx = c.X;
            float cy = c.Y;

            dd1 = sqr(cx - p[0].X) + sqr(cy - p[0].Y);
            dd2 = sqr(cx - p[1].X) + sqr(cy - p[1].Y);
            dd3 = sqr(cx - p[2].X) + sqr(cy - p[2].Y);
            dd4 = sqr(cx - p[3].X) + sqr(cy - p[3].Y);

            return dd1.AreSimilar(threshold, dd2, dd3, dd4) &&
                dd2.AreSimilar(threshold, dd3, dd4) &&
                dd3.AreSimilar(threshold, dd4);
        }

        public static Point FindCenter(this Point[] points)
        {
            var sumx = points.Select(p => p.X).Sum();
            var sumy = points.Select(p => p.Y).Sum();
            return new Point(sumx / points.Length, sumy / points.Length);
        }

        public static float DistanceTo(this Point p, Point d)
        {
            return (float) Math.Sqrt(sqr(p.X - d.X) + sqr(p.Y - d.Y));
        }

        public static float Magnitude(this Point p)
        {
            return Point.Empty.DistanceTo(p);
        }

        public static bool AreSimilar(this float orig, float threshold, params float[] values)
        {
            foreach(var v in values)
            {
                if (orig > v + threshold || orig < v - threshold)
                    return false;
            }
            return true;
        }

        private static float sqr(float a)
        {
            return a*a;
        }
    }
}
