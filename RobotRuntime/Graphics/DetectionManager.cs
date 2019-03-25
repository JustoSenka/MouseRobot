using RobotRuntime.Abstractions;
using RobotRuntime.Settings;
using RobotRuntime.Utils;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RobotRuntime.Graphics
{
    public class DetectionManager : IDetectionManager
    {
        private const float k_RectangleTolerance = 0.4f;

        private string m_DefaultDetector = DetectorNamesHardcoded.Default;

        private IProfiler Profiler;
        private IFeatureDetectorFactory FeatureDetectorFactory;
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
        /// Returns center points of all found images.
        /// Thread safe, does not use any class fields
        /// </summary>
        public async Task<Point[]> FindImage(Bitmap sampleImage, string detectorName, int timeout)
        {
            if (timeout < 50)
            {
                Logger.Log(LogType.Error, "Timeout is too small, image detection will early out. Timeout (ms): " + timeout);
                return null;
            }

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

            var points = await FindPointsInternal(clonedSampleImage, detector, timeout);

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
        /// Returns an array of center points of images found
        /// </summary>
        private async Task<Point[]> FindPointsInternal(Bitmap sampleImage, FeatureDetector detector, int timeout)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);

            return await Task.Run(async () =>
            {
                var success = false;
                while (true)
                {
                    Profiler.Start("ImageManager_CloneScreen");
                    var observedImage = BitmapUtility.TakeScreenshot();
                    Profiler.Stop("ImageManager_CloneScreen");

                    Profiler.Start("ImageManager_FindImage");
                    var points = FindImagePositions(detector, sampleImage, observedImage);
                    Profiler.Stop("ImageManager_FindImage");

                    success = ValidatePointsCorrectness(points, sampleImage.Width, sampleImage.Height);

                    if (success)
                        return points.Select(p => p.FindCenter()).ToArray();

                    if (cts.Token.IsCancellationRequested)
                        return null;

                    await Task.Delay(40);
                }
            }, cts.Token);
        }

        /// <summary>
        /// Returns array of positions for given image and detector
        /// </summary>
        private static Point[][] FindImagePositions(FeatureDetector detector, Bitmap sampleImage, Bitmap observedImage)
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
