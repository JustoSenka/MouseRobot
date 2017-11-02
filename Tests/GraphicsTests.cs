using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Emgu.CV;
using Emgu.CV.Util;
using RobotRuntime.Graphics;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using System.Drawing;
using Robot;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class GraphicsTests
    {
        [TestMethod]
        public void FindImageInsideAnotherImage()
        {
            MouseRobot.Instance.ForceInit();
            AssetManager.Instance.Refresh();

            var modelImage = new Mat(AssetManager.Instance.GetAsset("Images", "UnityButton").Path);
            var observedImage = new Mat(AssetManager.Instance.GetAsset("Images", "UnityCollab").Path);

            long time;
            var list = FindMatches(modelImage, observedImage, out time);

            Assert.IsTrue(true);
        }

        public static List<PointF[]> FindMatches(Mat modelImage, Mat observedImage, out long matchTime)
        {
            Mat homography;
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;

            var list = new List<PointF[]>();
            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
            {
                Mat mask;
                FeatureDetection.FindMatch(modelImage, observedImage, out matchTime, out modelKeyPoints, out observedKeyPoints, matches,
                   out mask, out homography);

                for (int i = 0; i < matches.Size; i++)
                {
                    var arrayOfMatches = matches[i].ToArray();
                    if (mask.GetData(i)[0] == 0) continue;
                    foreach (var match in arrayOfMatches)
                    {
                        var matchingModelKeyPoint = modelKeyPoints[match.TrainIdx];
                        var matchingObservedKeyPoint = observedKeyPoints[match.QueryIdx];
                        Console.WriteLine("Model coordinate '" + matchingModelKeyPoint.Point + "' matches observed coordinate '" + matchingObservedKeyPoint.Point + "'.");
                    }

                    list.Add(arrayOfMatches.Select(m => observedKeyPoints[m.QueryIdx].Point).ToArray());
                }
            }
            return list;
        }

        public static Point[] FindPoints(Mat modelImage, Mat observedImage, out long matchTime)
        {
            Mat homography;
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
            {
                Mat mask;
                FeatureDetection.FindMatch(modelImage, observedImage, out matchTime, out modelKeyPoints, out observedKeyPoints, matches,
                   out mask, out homography);

                //Draw the matched keypoints
                Mat result = new Mat();
                Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints,
                   matches, result, new MCvScalar(255, 255, 255), new MCvScalar(255, 255, 255), mask);

                Point[] points = null;
                if (homography != null)
                {
                    //draw a rectangle along the projected model
                    Rectangle rect = new Rectangle(Point.Empty, modelImage.Size);
                    PointF[] pts = new PointF[]
               {
                  new PointF(rect.Left, rect.Bottom),
                  new PointF(rect.Right, rect.Bottom),
                  new PointF(rect.Right, rect.Top),
                  new PointF(rect.Left, rect.Top)
               };
                    pts = CvInvoke.PerspectiveTransform(pts, homography);

                    points = Array.ConvertAll<PointF, Point>(pts, Point.Round);

                }

                return points;
            }
        }
    }
}
