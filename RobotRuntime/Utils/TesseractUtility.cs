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
        public static Rectangle GetPositionOfTextFromImage(Bitmap image, string text, IEqualityComparer<string> equalityComparer)
        {
            var tuple = GetAllTextBlocks(image).FirstOrDefault(b => equalityComparer.Equals(b.text, text));
            return tuple == default ? tuple.rect : default;
        }

        public static IEnumerable<(string text, Rectangle rect)> GetAllTextBlocks(Bitmap image)
        {
            var img = new Image<Bgr, Byte>(image);
            var smallImagesWithText = GetListOfImagesWithText(img);
            var sentences = smallImagesWithText.Select(i => GetTextFromImage(i.Item1));
            return sentences.Zip(smallImagesWithText, (s, i) => (text: s, rect: i.Item2));
        }

        static TesseractEngine engine = new TesseractEngine(Path.Combine(Paths.ApplicationInstallPath, @"tessdata"), "eng", EngineMode.Default);

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

        private static IEnumerable<string> IterateSentence(ResultIterator iter)
        {
            iter.Begin();
            do
            {
                var word = iter.GetText(PageIteratorLevel.Word);
                word = CleanWord(word);

                // skip short words
                if (word == null || word.Length < 2)
                    continue;

                yield return word;
            } while (iter.Next(PageIteratorLevel.Word));
        }

        private static bool WordIsClean(string word)
        {
            if (Regex.IsMatch(word, @"^[a-zA-Z]+$"))
                return true;
            return false;
        }
        private static string CleanWord(string word)
        {
            if (string.IsNullOrEmpty(word))
                return "";

            return Regex.Replace(word, @"[^a-zA-Z]", "");
        }

        private static IEnumerable<(Bitmap img, Rectangle rect)> GetListOfImagesWithText(Image<Bgr, Byte> img)
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
                {
                    var rect = ExtendRectangle(brect, img.Width, img.Height, 4);
                    var croppedImg = BitmapUtility.CropImageFromPoint(img.Bitmap, rect.Location, new Point(rect.X + rect.Width, rect.Y + rect.Height));
                    yield return (img: croppedImg, rect);
                }
            }
        }

        private static Rectangle ExtendRectangle(Rectangle r, int maxWidth, int maxHeight, int amountOfPixels)
        {
            //r.X = r.X - amountOfPixels > 0 ? r.X - amountOfPixels : 0;
            r.Y = r.Y - amountOfPixels > 0 ? r.Y - amountOfPixels : 0;

            //r.Width = r.Width + 2 * amountOfPixels <= maxWidth ? r.Width + 2 * amountOfPixels : maxWidth;
            r.Height = r.Height + 2 * amountOfPixels <= maxHeight ? r.Height + 2 * amountOfPixels : maxHeight;

            return r;
        }

        private static IEnumerable<Rectangle> FindTextBlocksFromImage(Image<Bgr, Byte> img)
        {
            /*
             1. Edge detection (sobel)
             2. Dilation (10,1)
             3. FindContours
             4. Geometrical Constrints
             */

            //sobel
            Image<Gray, byte> sobel = img.Convert<Gray, byte>().Sobel(1, 0, 3).AbsDiff(new Gray(0.0)).Convert<Gray, byte>().ThresholdBinary(new Gray(50), new Gray(255));
            Mat SE = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new Size(10, 2), new Point(-1, -1));
            sobel = sobel.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Dilate, SE, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Reflect, new MCvScalar(255));
            Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
            Mat m = new Mat();

            CvInvoke.FindContours(sobel, contours, m, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            List<Rectangle> list = new List<Rectangle>();

            for (int i = 0; i < contours.Size; i++)
            {
                Rectangle brect = CvInvoke.BoundingRectangle(contours[i]);

                double ar = brect.Width / brect.Height;
                if (ar > 2 && brect.Width > 25 && brect.Height > 8 && brect.Height < 100)
                {
                    list.Add(brect);
                }
            }
            return list;
        }
    }
}