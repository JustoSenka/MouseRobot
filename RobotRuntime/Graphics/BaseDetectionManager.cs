using RobotRuntime.Abstractions;
using RobotRuntime.Settings;
using RobotRuntime.Utils;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RobotRuntime.Graphics
{
    public abstract class BaseDetectionManager : IDetectionManager
    {
        protected const float k_RectangleTolerance = 0.4f;

        protected virtual string DefaultDetector { get; set; } = DetectorNamesHardcoded.Default;

        /// <summary>
        /// Apply detection settings, default detector will be chosen according these settings
        /// </summary>
        public virtual void ApplySettings(DetectionSettings settings) { }

        /// <summary>
        /// If user specifies detector which is not "default", give user detector
        /// Else, give detector which was set in global settings file
        /// </summary>
        protected virtual string GetPreferredDetector(string userSpecifiedDetector)
        {
            return userSpecifiedDetector == DetectorNamesHardcoded.Default ? DefaultDetector : userSpecifiedDetector;
        }

        /// <summary>
        /// Returns center points of all found detectables.
        /// Thread safe, does not use any class fields
        /// </summary>
        public async Task<Point[]> Find(Detectable detectable, string detectorName, int timeout)
        {
            var rects = await FindRects(detectable, detectorName, timeout);
            return rects?.Select(p => p.FindCenter()).ToArray();
        }

        /// <summary>
        /// Returns center points of all found detectables.
        /// Thread safe, does not use any class fields
        /// </summary>
        public async Task<Point[]> Find(Detectable detectable, Bitmap observedImage, string detectorName)
        {
            var rects = await FindRects(detectable, observedImage, detectorName);
            return rects?.Select(p => p.FindCenter()).ToArray();
        }

        /// <summary>
        /// Returns rects of all found detectables. Constantly takes screenshots until timeout finishes or detectable is found.
        /// If timeout is 0, will try only once with first taken screenshot
        /// If timeout is bigger but still smaller than 100ms there is a big chance detectable will not be found because it takes longer to take screenshot
        /// </summary>
        public async Task<Point[][]> FindRects(Detectable detectable, string detectorName, int timeout)
        {
            return await FindRectsInternal(detectable, detectorName, timeout);
        }

        /// <summary>
        /// Returns rects of all found detectables. Tries only once with compare with given screenshot.
        /// Faster than other overload.
        /// </summary>
        public async Task<Point[][]> FindRects(Detectable detectable, Bitmap observedImage, string detectorName)
        {
            return await FindRectsInternal(detectable, detectorName, 0, observedImage);
        }

        /// <summary>
        /// Used to do preparations such as detectable caching for detectables
        /// </summary>
        protected virtual bool BeforeStartingSearch(Detectable detectable, string detectorName, int timeout, Bitmap observedImage = null) => true;

        protected abstract bool FindRectsSync(Detectable detectable, string detectorName, Bitmap observedImage, out Point[][] points);

        /// <summary>
        /// Runs while loop in a task to find detectable, while constantly taking screenshots
        /// Or does the operation once if timeout was 0 or user provided his own observed image
        /// </summary>
        private async Task<Point[][]> FindRectsInternal(Detectable detectable, string detectorName, int timeout, Bitmap observedImage = null)
        {
            if (detectable.Value == null)
                return null;

            var shouldTakeAScreenshot = timeout > 0 || observedImage == null;
            var screenshotToUse = shouldTakeAScreenshot ? observedImage : BitmapUtility.TakeScreenshot();

            // Used to do preparations such as detectable caching for images
            var shouldContinue = BeforeStartingSearch(detectable, detectorName, timeout, screenshotToUse);
            if (!shouldContinue)
                return null;

            var points = await RunLoopForRectSearch(detectable, detectorName, timeout, screenshotToUse);

            // Detectable was not found
            if (points == null || points.Length == 0)
                return null;

            return points;
        }

        /// <summary>
        /// Runs while loop in a task to find detectable, while constantly taking screenshots
        /// Or does the operation once if timeout was 0 or user provided his own observed image
        /// </summary>
        private async Task<Point[][]> RunLoopForRectSearch(Detectable detectable, string detectorName, int timeout, Bitmap screenshotToUse)
        {
            // If timeout was 0 or user provided his own observed image, do the search only once without a task
            if (timeout == 0 || screenshotToUse != null)
            {
                var success = FindRectsSync(detectable, detectorName, screenshotToUse, out Point[][] points);
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
                        screenshotToUse = BitmapUtility.TakeScreenshot();
                        var success = FindRectsSync(detectable, detectorName, screenshotToUse, out Point[][] points);

                        if (success)
                            return points;

                        if (cts.Token.IsCancellationRequested)
                            return null;

                        await Task.Delay(40);
                    }
                }, cts.Token);
            }
        }

        #region STATIC Methods

        /// <summary>
        /// Returns false if points array is bad, or if it is not square like
        /// </summary>
        protected static bool ValidatePointsCorrectness(Point[][] points, int sampleImageWith, int sampleImageHeight)
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
        
        #endregion
    }
}
