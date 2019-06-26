using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Diagnostics;
using Tesseract;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;

namespace RobotRuntime.Utils
{
    public class TesseractUtility
    {
        private static TesseractEngine engine = new TesseractEngine(Path.Combine(Paths.ApplicationInstallPath, @"tessdata"), "eng", EngineMode.Default);

        public static Rectangle GetPositionOfTextFromImage(Bitmap image, string text, IEqualityComparer<string> equalityComparer)
        {
            var tuple = CollectAllBlocksAndGetTextFromInside(image).FirstOrDefault(b => equalityComparer.Equals(b.text, text));
            return tuple != default ? tuple.rect : default;
        }

        public static IEnumerable<(string text, Rectangle rect)> CollectAllBlocksAndGetTextFromInside(Bitmap bitmap)
        {
            var img = new Image<Bgr, Byte>(bitmap);
            var textBlocks = CollectBlocksWhichContainAnyText(img);
            return GetAllTextFromRects(img, textBlocks);
        }

        public static IEnumerable<(string text, Rectangle rect)> GetAllTextFromRects(Bitmap bitmap, IEnumerable<Rectangle> rects)
        {
            return GetAllTextFromRects(new Image<Bgr, Byte>(bitmap), rects);
        }

        public static IEnumerable<(string text, Rectangle rect)> GetAllTextFromRects(Image<Bgr, Byte> image, IEnumerable<Rectangle> rects)
        {
            var smallImagesOfTextBlocks = rects.Select(rect => (rect, img: CropImageToRectangle(rect, image)));

            var sentences = smallImagesOfTextBlocks.Select(i => GetTextFromImage(i.img));
            var zip = sentences.Zip(smallImagesOfTextBlocks, (s, i) => (text: s, i.rect));
                
            return zip.Where(z => !string.IsNullOrWhiteSpace(z.text));
        }

        public static string GetTextFromImage(Bitmap image)
        {
            try
            {
                using (var page = engine.Process(image, PageSegMode.Count))
                {
                    using (var iter = page.GetIterator())
                    {
                        var words = IterateSentence(iter);
                        var sentence = string.Join(" ", words);
                        return sentence;
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Console.WriteLine("Unexpected Error: " + e.Message);
                Console.WriteLine("Details: ");
                Console.WriteLine(e.ToString());
                return "";
            }
        }

        public static IEnumerable<Rectangle> CollectBlocksWhichContainAnyText(Bitmap img)
        {
            return CollectBlocksWhichContainAnyText(new Image<Bgr, Byte>(img));
        }

        private static IEnumerable<Rectangle> CollectBlocksWhichContainAnyText(Image<Bgr, Byte> img)
        {
            var sobel = img.Convert<Gray, byte>().Sobel(1, 0, 3).AbsDiff(new Gray(0.0)).Convert<Gray, byte>().ThresholdBinary(new Gray(100), new Gray(255));
            var SE = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new Size(10, 1), new Point(-1, -1));
            sobel = sobel.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Dilate, SE, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Reflect, new MCvScalar(255));
            var contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
            var m = new Mat();

            CvInvoke.FindContours(sobel, contours, m, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < contours.Size; i++)
            {
                Rectangle brect = CvInvoke.BoundingRectangle(contours[i]);

                double ar = brect.Width / brect.Height;
                if (ar > 1.4 && brect.Width > 20 && brect.Height > 6 && brect.Height < 100)
                    yield return ExtendRectangle(brect, img.Width, img.Height, 4);
            }
        }

        private static IEnumerable<string> IterateSentence(ResultIterator iter)
        {
            iter.Begin();
            do
            {
                var word = iter.GetText(PageIteratorLevel.Word);
                // word = CleanWord(word);

                // skip short words
                if (string.IsNullOrWhiteSpace(word) || word.Length <= 1)
                    continue;

                yield return word;
            } while (iter.Next(PageIteratorLevel.Word));
        }

        private static Bitmap CropImageToRectangle(Rectangle rect, Image<Bgr, Byte> img)
        {
            var croppedImg = BitmapUtility.CropImageFromPoint(img.Bitmap, rect.Location, new Point(rect.X + rect.Width, rect.Y + rect.Height));
            croppedImg = ResizeAndImprove(croppedImg);
            return croppedImg;
        }

        private static Rectangle ExtendRectangle(Rectangle r, int maxWidth, int maxHeight, int amountOfPixels)
        {
            //r.X = r.X - amountOfPixels > 0 ? r.X - amountOfPixels : 0;
            r.Y = r.Y - amountOfPixels > 0 ? r.Y - amountOfPixels : 0;

            //r.Width = r.Width + 2 * amountOfPixels <= maxWidth ? r.Width + 2 * amountOfPixels : maxWidth;
            r.Height = r.Height + 2 * amountOfPixels <= maxHeight ? r.Height + 2 * amountOfPixels : maxHeight;

            return r;
        }

        private static Bitmap ResizeAndImprove(Bitmap inImg)
        {
            var outImg = new Image<Bgr, byte>(inImg).Resize(2, Emgu.CV.CvEnum.Inter.Cubic);
            return outImg.Convert<Gray, byte>().ThresholdBinary(new Gray(130), new Gray(255)).Bitmap;
        }

        private static string CleanWord(string word)
        {
            if (string.IsNullOrEmpty(word))
                return "";

            return Regex.Replace(word, @"[^a-zA-Z0-9]", "");
        }
    }
}
