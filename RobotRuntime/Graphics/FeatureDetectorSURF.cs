using System.Diagnostics;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.Cuda;
using Emgu.CV.XFeatures2D;
using System.Collections.Generic;
using System;
using RobotRuntime.Utils;

namespace RobotRuntime.Graphics
{
    public class FeatureDetectorSURF : FeatureDetector
    {
        public override string Name { get { return "SURF"; } }

        protected override float MaxScaleDownFactor { get { return 0.4f; } }
        protected override int MinimumImageScaleSize { get { return 130; } }

        public override IEnumerable<Point[]> FindImageMultiplePos(Bitmap sampleImage, Bitmap observedImage)
        {
            yield return FindImagePos(sampleImage, observedImage); // SURF cannot find multiple images
        }

        public override Point[] FindImagePos(Bitmap sampleImage, Bitmap observedImage)
        {
            var o = observedImage.ToImage();
            var s = sampleImage.ToImage();

            var scale = SmartResize(ref o, ref s);

            return FindImageInternal(s.Mat, o.Mat).Scale(1 / scale).ToPoint();
        }

        private static PointF[] FindImageInternal(Mat sampleImage, Mat observedImage)
        {
            Mat mask;
            Mat homography;
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;

            PointF[] points = new PointF[0];
            using (VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch())
            {
                FindMatch(sampleImage, observedImage, out modelKeyPoints, out observedKeyPoints, matches, out mask, out homography);

                if (homography != null)
                {
                    var rect = new Rectangle(Point.Empty, sampleImage.Size);
                    var pts = rect.ToPointF();
                    points = CvInvoke.PerspectiveTransform(pts, homography);
                }
            }
            return points;
        }

        private static bool FindMatch(Mat modelImage, Mat observedImage, out VectorOfKeyPoint modelKeyPoints, out VectorOfKeyPoint observedKeyPoints, VectorOfVectorOfDMatch matches, out Mat mask, out Mat homography)
        {
            int k = 2;
            double uniquenessThreshold = 0.8;
            double hessianThresh = 300;

            homography = null;

            modelKeyPoints = new VectorOfKeyPoint();
            observedKeyPoints = new VectorOfKeyPoint();

            if (CudaInvoke.HasCuda)
                homography = FindMatchWithCuda(modelImage, observedImage, modelKeyPoints, observedKeyPoints, matches, out mask, homography, k, uniquenessThreshold, hessianThresh);
            else
                homography = FindMatchWithoutCuda(modelImage, observedImage, modelKeyPoints, observedKeyPoints, matches, out mask, homography, k, uniquenessThreshold, hessianThresh);

            return homography != null;
        }

        private static Mat FindMatchWithoutCuda(Mat modelImage, Mat observedImage, VectorOfKeyPoint modelKeyPoints, VectorOfKeyPoint observedKeyPoints, VectorOfVectorOfDMatch matches, out Mat mask, Mat homography, int k, double uniquenessThreshold, double hessianThresh)
        {
            using (UMat uModelImage = modelImage.GetUMat(AccessType.Read))
            using (UMat uObservedImage = observedImage.GetUMat(AccessType.Read))
            {
                SURF surfCPU = new SURF(hessianThresh, upright: true);

                UMat modelDescriptors;
                if (!FindDescriptors(surfCPU, modelKeyPoints, uModelImage, out modelDescriptors))
                {
                    Logger.Log(LogType.Error, "Feature Descriptor for Model image is empty. Is the image too small?");
                    return mask = null;
                }

                UMat observedDescriptors;
                if (!FindDescriptors(surfCPU, observedKeyPoints, uObservedImage, out observedDescriptors))
                {
                    Logger.Log(LogType.Error, "Feature Descriptor for Observed image is empty. Is the image too small?");
                    return mask = null;
                }

                BFMatcher matcher = new BFMatcher(DistanceType.L2);
                matcher.Add(modelDescriptors);
                matcher.KnnMatch(observedDescriptors, matches, k, null);

                mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                mask.SetTo(new MCvScalar(255));
                Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                int nonZeroCount = CvInvoke.CountNonZero(mask);
                if (nonZeroCount >= 4)
                {
                    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints, matches, mask, 1.5, 20);
                    if (nonZeroCount >= 4)
                        homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints, observedKeyPoints, matches, mask, 2);
                }
            }

            return homography;
        }

        private static bool FindDescriptors(Feature2D feature2D, VectorOfKeyPoint modelKeyPoints, UMat uModelImage, out UMat modelDescriptors)
        {
            modelDescriptors = new UMat();
            feature2D.DetectAndCompute(uModelImage, null, modelKeyPoints, modelDescriptors, false);
            return modelDescriptors.Size.Height + modelDescriptors.Size.Width > 1;
        }

        private static Mat FindMatchWithCuda(Mat modelImage, Mat observedImage, VectorOfKeyPoint modelKeyPoints, VectorOfKeyPoint observedKeyPoints, VectorOfVectorOfDMatch matches, out Mat mask, Mat homography, int k, double uniquenessThreshold, double hessianThresh)
        {
            CudaSURF surfCuda = new CudaSURF((float)hessianThresh);
            using (GpuMat gpuModelImage = new GpuMat(modelImage))
            //extract features from the object image
            using (GpuMat gpuModelKeyPoints = surfCuda.DetectKeyPointsRaw(gpuModelImage, null))
            using (GpuMat gpuModelDescriptors = surfCuda.ComputeDescriptorsRaw(gpuModelImage, null, gpuModelKeyPoints))
            using (CudaBFMatcher matcher = new CudaBFMatcher(DistanceType.L2))
            {
                surfCuda.DownloadKeypoints(gpuModelKeyPoints, modelKeyPoints);

                // extract features from the observed image
                using (GpuMat gpuObservedImage = new GpuMat(observedImage))
                using (GpuMat gpuObservedKeyPoints = surfCuda.DetectKeyPointsRaw(gpuObservedImage, null))
                using (GpuMat gpuObservedDescriptors = surfCuda.ComputeDescriptorsRaw(gpuObservedImage, null, gpuObservedKeyPoints))
                //using (GpuMat tmp = new GpuMat())
                //using (Stream stream = new Stream())
                {
                    matcher.KnnMatch(gpuObservedDescriptors, gpuModelDescriptors, matches, k);

                    surfCuda.DownloadKeypoints(gpuObservedKeyPoints, observedKeyPoints);

                    mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                    mask.SetTo(new MCvScalar(255));
                    Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                    int nonZeroCount = CvInvoke.CountNonZero(mask);
                    if (nonZeroCount >= 4)
                    {
                        nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints,
                           matches, mask, 1.5, 20);
                        if (nonZeroCount >= 4)
                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints,
                               observedKeyPoints, matches, mask, 2);
                    }
                }
            }

            return homography;
        }
    }
}
