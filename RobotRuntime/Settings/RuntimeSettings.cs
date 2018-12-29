using RobotRuntime.Abstractions;
using RobotRuntime.IO;
using RobotRuntime.Utils;
using System.IO;

namespace RobotRuntime.Settings
{
    public class RuntimeSettings : IRuntimeSettings
    {
        IScreenStateThread ScreenStateThread;
        IFeatureDetectionThread FeatureDetectionThread;
        public RuntimeSettings(IScreenStateThread ScreenStateThread, IFeatureDetectionThread FeatureDetectionThread)
        {
            this.ScreenStateThread = ScreenStateThread;
            this.FeatureDetectionThread = FeatureDetectionThread;
        }

        /// <summary>
        /// Will be replaced when settings per command are implemented.
        /// </summary>
        public void LoadSettingsHardcoded()
        {
            var path = Path.Combine(Paths.RoamingAppdataPath, "FeatureDetectionSettings.config");
            var settings = new JsonObjectIO().LoadObject<FeatureDetectionSettings>(path);
            ApplySettings(settings);
        }

        // TODO: Probably add read here, since runtime also needs to read settings from file somehow imo
        // Probably settings should be per command, maybe diff commands should have diff settings
        public void ApplySettings(FeatureDetectionSettings settings)
        {
            ScreenStateThread.FPS = settings.ScreenImageUpdateFPS;
            FeatureDetectionThread.FPS = settings.ImageDetectionFPS;
            FeatureDetectionThread.DetectorName = settings.DetectionMode;
        }
    }
}
