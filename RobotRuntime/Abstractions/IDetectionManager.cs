using RobotRuntime.Graphics;
using RobotRuntime.Settings;
using System.Drawing;
using System.Threading.Tasks;

namespace RobotRuntime.Abstractions
{
    public interface IDetectionManager
    {
        void ApplySettings(FeatureDetectionSettings settings);

        /// <summary>
        /// Returns center points of all found detectables.
        /// Thread safe, does not use any class fields
        /// </summary>
        Task<Point[]> Find(Detectable detectable, Bitmap observedImage, string detectorName);

        /// <summary>
        /// Returns center points of all found detectables.
        /// Thread safe, does not use any class fields
        /// </summary>
        Task<Point[]> Find(Detectable detectable, string detectorName, int timeout);

        /// <summary>
        /// Returns rects of all found detectables. Tries only once with compare with given screenshot.
        /// Thread safe, Faster than other overload.
        /// </summary>
        Task<Point[][]> FindRects(Detectable detectable, Bitmap observedImage, string detectorName);

        /// <summary>
        /// Returns rects of all found detectables. Constantly takes screenshots until timeout finishes or image is found.
        /// If timeout is 0, will try only once with first taken screenshot
        /// If timeout is bigger but still smaller than 100ms there is a big chance image will not be found because it takes longer to take screenshot
        /// Thread safe
        /// </summary>
        Task<Point[][]> FindRects(Detectable detectable, string detectorName, int timeout);
    }
}
