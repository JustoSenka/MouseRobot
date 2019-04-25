using RobotRuntime.Abstractions;
using RobotRuntime.Settings;
using RobotRuntime.Utils;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Lifetime;

namespace RobotRuntime.Graphics
{
    [RegisterTypeToContainer(typeof(IDetectionManager), typeof(ContainerControlledLifetimeManager))]
    public class DetectionManager : IDetectionManager
    {
        private const float k_RectangleTolerance = 0.4f;
        private string m_DefaultDetector = DetectorNamesHardcoded.Default;

        private readonly IProfiler Profiler;
        private readonly IFeatureDetectorFactory FeatureDetectorFactory;
        public DetectionManager(IProfiler Profiler, IFeatureDetectorFactory FeatureDetectorFactory)
        {
            this.Profiler = Profiler;
            this.FeatureDetectorFactory = FeatureDetectorFactory;
        }

        /// <summary>
        /// Apply detection settings, default detector will be chosen according these settings
        /// </summary>
        public void ApplySettings(FeatureDetectionSettings settings)
        {
            m_DefaultDetector = settings.DetectionMode;
        }

        /// <summary>
        /// Returns rects of all found images. Constantly takes screenshots until timeout finishes or image is found.
        /// If timeout is 0, will try only once with first taken screenshot
        /// If timeout is bigger but still smaller than 100ms there is a big chance image will not be found because it takes longer to take screenshot
        /// </summary>
        public async Task<Point[][]> FindImageRects(Bitmap sampleImage, string detectorName, int timeout)
        {
            return await FindImageRectsInternal(sampleImage, detectorName, timeout);
        }

        /// <summary>
        /// Returns rects of all found images. Tries only once with compare with given screenshot.
        /// Faster than other overload.
        /// </summary>
        public async Task<Point[][]> FindImageRects(Bitmap sampleImage, Bitmap observedImage, string detectorName)
        {
            return await FindImageRectsInternal(sampleImage, detectorName, 0, observedImage);
        }

        /// <summary>
        /// Returns center points of all found images.
        /// Thread safe, does not use any class fields
        /// </summary>
        public async Task<Point[]> FindImage(Bitmap sampleImage, string detectorName, int timeout)
        {
            var rects = await FindImageRects(sampleImage, detectorName, timeout);
            return rects?.Select(p => p.FindCenter()).ToArray();
        }

        /// <summary>
        /// Returns center points of all found images.
        /// Thread safe, does not use any class fields
        /// </summary>
        public async Task<Point[]> FindImage(Bitmap sampleImage, Bitmap observedImage, string detectorName)
        {
            var rects = await FindImageRects(sampleImage, observedImage, detectorName);
            return rects?.Select(p => p.FindCenter()).ToArray();
        }

        private async Task<Point[][]> FindImageRectsInternal(Bitmap sampleImage, string detectorName, int timeout, Bitmap observedImage = null)
        {
            if (sampleImage == null)
            {
                Logger.Log(LogType.Error, "Sample image was null. Did it fail to find correct asset from GUID?");
                return null;
            }

            Bitmap clonedSampleImage = null;
            lock (sampleImage)
            {
                clonedSampleImage = BitmapUtility.Clone32BPPBitmap(sampleImage, new Bitmap(sampleImage.Width, sampleImage.Height));
            }

            if (clonedSampleImage == null)
            {
                Logger.Log(LogType.Error, "Cloned sample image was null. That's a bug, please report it.");
                return null;
            }

            var preferredDetectorName = GetPreferredDetector(detectorName);
            var detector = FeatureDetectorFactory.Create(preferredDetectorName);
            if (detector == null)
                return null;

            var points = await FindPointRectsInternal(clonedSampleImage, detector, timeout);

            // Image was not found
            if (points == null || points.Length == 0)
                return null;

            return points;
        }

        /// <summary>
        /// If user specifies detector which is not "default", give user detector
        /// Else, give detector which was set in global settings file
        /// </summary>
        private string GetPreferredDetector(string userSpecifiedDetector)
        {
            return userSpecifiedDetector == DetectorNamesHardcoded.Default ? m_DefaultDetector : userSpecifiedDetector;
        }

        /// <summary>
        /// Runs while loop in a task to find image, while constantly taking screenshots
        /// Or does the operation once if timeout was 0 or user provided his own observed image
        /// </summary>
        private async Task<Point[][]> FindPointRectsInternal(Bitmap sampleImage, FeatureDetector detector, int timeout, Bitmap observedImage = null)
        {
            if (timeout == 0)
                observedImage = BitmapUtility.TakeScreenshot();

            // If timeout was 0 or user provided his own observed image, do the search only once without a task
            if (observedImage != null)
            {
                var points = FindImageRectPositions(detector, sampleImage, observedImage);
                var success = ValidatePointsCorrectness(points, sampleImage.Width, sampleImage.Height);
                return success ? points : null;
            }
            else
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(timeout);

                return await Task.Run(async () =>
                {
                    while (true)
                    {
                        observedImage = BitmapUtility.TakeScreenshot();
                        var points = FindImageRectPositions(detector, sampleImage, observedImage);
                        var success = ValidatePointsCorrectness(points, sampleImage.Width, sampleImage.Height);

                        if (success)
                            return points;

                        if (cts.Token.IsCancellationRequested)
                            return null;

                        await Task.Delay(40);
                    }
                }, cts.Token);
            }
        }

        /// <summary>
        /// Returns array of positions for given image and detector
        /// </summary>
        private static Point[][] FindImageRectPositions(FeatureDetector detector, Bitmap sampleImage, Bitmap observedImage)
        {
            if (detector.SupportsMultipleMatches)
                return detector.FindImageMultiplePos(sampleImage, observedImage).ToArray();
            else
                return new[] { detector.FindImagePos(sampleImage, observedImage) };
        }

        /// <summary>
        /// Returns false if points array is bad, or if it is not square like
        /// </summary>
        private static bool ValidatePointsCorrectness(Point[][] points, int sampleImageWith, int sampleImageHeight)
        {
            if (points == null || points.Length == 0 || points[0] == null || points[0].Length < 4)
                return false;

            if (points.GetLength(0) == 1 && points[0].Length == 4)
            {
                var threshold = k_RectangleTolerance * new Point(sampleImageWith + sampleImageHeight).Magnitude() / 2;
                if (!points[0].IsRectangleish(threshold))
                    return false;
            }

            return true;
        }
    }
}
