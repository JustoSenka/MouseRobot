using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Robot.Utils
{
    static class BitmapUtility
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
    }
}
