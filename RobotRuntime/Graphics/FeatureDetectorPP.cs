using System.Drawing;
using System;
using System.Collections.Generic;
using RobotRuntime.Utils;
using System.Drawing.Imaging;
using System.Linq;

namespace RobotRuntime.Graphics
{
    /// <summary>
    /// Pixel Perfect feature detector, quite fast, supports multiple matches.
    /// Singe pixel mismatch will break it.
    /// Vulnerable to big images, everything whats above 300x300 is too much
    /// </summary>
    public class FeatureDetectorPP : FeatureDetector
    {
        private const float Threshold = 0.99f;

        public override bool SupportsMultipleMatches { get { return true; } }

        public override IEnumerable<Point[]> FindImageMultiplePos(Bitmap smallBmp, Bitmap bigBmp)
        {
            return FindImagePositionInternal(smallBmp, bigBmp, false);
        }

        public override Point[] FindImagePos(Bitmap smallBmp, Bitmap bigBmp)
        {
            return FindImagePositionInternal(smallBmp, bigBmp, true).FirstOrDefault();
        }

        private static IEnumerable<Point[]> FindImagePositionInternal(Bitmap smallBmp, Bitmap bigBmp, bool returnFirstFoundPos)
        {
            var smallData = smallBmp.LockBits(smallBmp.Bounds(), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var bigData = bigBmp.LockBits(bigBmp.Bounds(), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            int smallStride = smallData.Stride;
            int bigStride = bigData.Stride;

            int bigWidth = bigBmp.Width;
            int bigHeight = bigBmp.Height - smallBmp.Height + 1;
            int smallWidth = smallBmp.Width * 3;
            int smallHeight = smallBmp.Height;

            var list = new List<Point[]>();
            int margin = Convert.ToInt32(255.0 * (1f - Threshold));

            unsafe
            {
                byte* pSmall = (byte*)(void*)smallData.Scan0;
                byte* pBig = (byte*)(void*)bigData.Scan0;

                int smallOffset = smallStride - smallBmp.Width * 3;
                int bigOffset = bigStride - bigBmp.Width * 3;

                bool matchFound = true;

                for (int y = 0; y < bigHeight; y++)
                {
                    for (int x = 0; x < bigWidth; x++)
                    {
                        byte* pBigBackup = pBig;
                        byte* pSmallBackup = pSmall;

                        //Look for the small picture.
                        for (int i = 0; i < smallHeight; i++)
                        {
                            int j = 0;
                            matchFound = true;
                            for (j = 0; j < smallWidth; j++)
                            {
                                //With tolerance: pSmall value should be between margins.
                                int inf = pBig[0] - margin;
                                int sup = pBig[0] + margin;
                                if (sup < pSmall[0] || inf > pSmall[0])
                                {
                                    matchFound = false;
                                    break;
                                }

                                pBig++;
                                pSmall++;
                            }

                            if (!matchFound) break;

                            //We restore the pointers.
                            pSmall = pSmallBackup;
                            pBig = pBigBackup;

                            //Next rows of the small and big pictures.
                            pSmall += smallStride * (1 + i);
                            pBig += bigStride * (1 + i);
                        }

                        if (matchFound)
                        {
                            list.Add(new Rectangle(new Point(x, y), smallBmp.Size).ToPoint());
                            if (returnFirstFoundPos)
                                break;
                        }

                        pBig = pBigBackup;
                        pSmall = pSmallBackup;
                        pBig += 3;
                    }

                    if (matchFound && returnFirstFoundPos)
                        break;

                    pBig += bigOffset;
                }
            }

            bigBmp.UnlockBits(bigData);
            smallBmp.UnlockBits(smallData);

            return list;
        }
    }
}
