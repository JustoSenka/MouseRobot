using RobotRuntime.Abstractions;

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


        // TODO: Probably add read here, since runtime also needs to read settings from file somehow imo
        // Probably settings should be per command, maybe diff commands should have diff settings
        public void ApplySettings(FeatureDetectionSettings settings)
        {
            ScreenStateThread.FPS = settings.ScreenImageUpdateFPS;
            FeatureDetectionThread.FPS = settings.ImageDetectionFPS;
            FeatureDetectionThread.DetectionMode = settings.DetectionMode;
        }
    }
}
