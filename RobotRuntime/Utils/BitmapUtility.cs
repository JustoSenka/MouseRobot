using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RobotRuntime.Utils
{
    public static class BitmapUtility
    {
        public unsafe static Bitmap Clone32BPPBitmap(Bitmap srcBitmap, Bitmap result)
        {
            Rectangle bmpBounds = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);
            BitmapData srcData = srcBitmap.LockBits(bmpBounds, ImageLockMode.ReadOnly, srcBitmap.PixelFormat);
            BitmapData resData = result.LockBits(bmpBounds, ImageLockMode.WriteOnly, result.PixelFormat);

            int* srcScan0 = (int*)srcData.Scan0;
            int* resScan0 = (int*)resData.Scan0;
            int numPixels = srcData.Stride / 4 * srcData.Height;
            try
            {
                for (int p = 0; p < numPixels; p++)
                {
                    resScan0[p] = srcScan0[p];
                }
            }
            finally
            {
                srcBitmap.UnlockBits(srcData);
                result.UnlockBits(resData);
            }

            return result;
        }

        /// <summary>
        /// Not tested to work
        /// </summary>
        public static Bitmap Copy32BPPBitmapSafe(Bitmap srcBitmap, Bitmap result)
        {
            //Bitmap result = new Bitmap(srcBitmap.Width, srcBitmap.Height, PixelFormat.Format32bppArgb);

            Rectangle bmpBounds = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);
            BitmapData srcData = srcBitmap.LockBits(bmpBounds, ImageLockMode.ReadOnly, srcBitmap.PixelFormat);
            BitmapData resData = result.LockBits(bmpBounds, ImageLockMode.WriteOnly, result.PixelFormat);

            Int64 srcScan0 = srcData.Scan0.ToInt64();
            Int64 resScan0 = resData.Scan0.ToInt64();
            int srcStride = srcData.Stride;
            int resStride = resData.Stride;
            int rowLength = Math.Abs(srcData.Stride);
            try
            {
                byte[] buffer = new byte[rowLength];
                for (int y = 0; y < srcData.Height; y++)
                {
                    Marshal.Copy(new IntPtr(srcScan0 + y * srcStride), buffer, 0, rowLength);
                    Marshal.Copy(buffer, 0, new IntPtr(resScan0 + y * resStride), rowLength);
                }
            }
            finally
            {
                srcBitmap.UnlockBits(srcData);
                result.UnlockBits(resData);
            }

            return result;
        }

        public static Image<Bgr, byte> ToImage(this Bitmap bmp)
        {
            return new Image<Bgr, byte>(bmp);
        }

        public static Bitmap TakeScreenshotOfSpecificRect(Point p1, Point p2)
        {
            var topLeft = new Point(p1.X < p2.X ? p1.X : p2.X, p1.Y < p2.Y ? p1.Y : p2.Y);
            var botRight = new Point(p1.X >= p2.X ? p1.X : p2.X, p1.Y >= p2.Y ? p1.Y : p2.Y);
            var rect = new Rectangle(0, 0, botRight.X - topLeft.X, botRight.Y - topLeft.Y);
            return TakeScreenshotOfSpecificRect(topLeft, rect.Size);
        }

        public static Bitmap TakeScreenshotOfSpecificRect(Point upperLeftPoint, Size size)
        {
            var screen = Screen.PrimaryScreen;
            var bmp = new Bitmap(size.Width, size.Height);
            using (var graphics = System.Drawing.Graphics.FromImage(bmp))
            {
                graphics.CopyFromScreen(upperLeftPoint, new Point(0, 0), size);
            }
            return bmp;
        }

        public static Bitmap TakeScreenshot()
        {
            var screen = Screen.PrimaryScreen;
            return TakeScreenshotOfSpecificRect(new Point(screen.Bounds.Left, screen.Bounds.Top), screen.Bounds.Size);
        }

        public static void TakeScreenshot(Bitmap dest)
        {
            var screen = Screen.PrimaryScreen;
            using (var graphics = System.Drawing.Graphics.FromImage(dest))
            {
                graphics.CopyFromScreen(new Point(screen.Bounds.Left, screen.Bounds.Top), new Point(0, 0), screen.Bounds.Size);
            }
        }

        public static Bitmap CropImageFromPoint(Bitmap source, Point center, int size)
        {
            return CropImageFromPoint(source, 
                new Point(center.X - size / 2, center.Y - size / 2), 
                new Point(center.X + size / 2, center.Y + size / 2));
        }

        public static Bitmap CropImageFromPoint(Bitmap source, Point from, Point to)
        {
            var topLeft = new Point(from.X < to.X ? from.X : to.X, from.Y < to.Y ? from.Y : to.Y);
            var botRight = new Point(from.X >= to.X ? from.X : to.X, from.Y >= to.Y ? from.Y : to.Y);
            var rect = new Rectangle(0, 0, botRight.X - topLeft.X, botRight.Y - topLeft.Y);

            var bmp = new Bitmap(rect.Width, rect.Height, source.PixelFormat);

            for (int x = 0; x < rect.Width; ++x)
                for (int y = 0; y < rect.Height; ++y)
                    bmp.SetPixel(x, y, source.GetPixel(topLeft.X + x, topLeft.Y + y));

            return bmp;
        }

        public static Bitmap ResizeBitmap(Bitmap bmp, Rectangle rect)
        {
            var newBmp = new Bitmap(rect.Width, rect.Height);
            using (var g = System.Drawing.Graphics.FromImage(newBmp))
            {
                g.DrawImage(bmp, new Rectangle(0, 0, newBmp.Width, newBmp.Height), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel);
            }
            return newBmp;
        }

        public static Rectangle GetRect(Point p1, Point p2)
        {
            var topLeft = new Point(p1.X < p2.X ? p1.X : p2.X, p1.Y < p2.Y ? p1.Y : p2.Y);
            var botRight = new Point(p1.X >= p2.X ? p1.X : p2.X, p1.Y >= p2.Y ? p1.Y : p2.Y);
            var rect = new Rectangle(0, 0, botRight.X - topLeft.X, botRight.Y - topLeft.Y);
            rect.Location = topLeft;
            return rect;
        }


        // Those Might be way faster than taking screenshot and getting pixels
        //   while you can take pixels from screen directly without taking screenshot
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr window);
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern uint GetPixel(IntPtr dc, int x, int y);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int ReleaseDC(IntPtr window, IntPtr dc);

        public static Color GetColorAt(int x, int y)
        {
            IntPtr desk = GetDesktopWindow();
            IntPtr dc = GetWindowDC(desk);
            int a = (int)GetPixel(dc, x, y);
            ReleaseDC(desk, dc);
            return Color.FromArgb(255, (a >> 0) & 0xff, (a >> 8) & 0xff, (a >> 16) & 0xff);
        }

        public static Rectangle Bounds(this Bitmap bmp)
        {
            return new Rectangle(0, 0, bmp.Width, bmp.Height);
        }
    }
}
