using RobotRuntime.Graphics;

namespace RobotRuntime.Settings
{
    public static class RuntimeSettings
    {
        // TODO: Probably add read here, since runtime also needs to read settings from file somehow imo
        // Probably settings should be per command, maybe diff commands should have diff settings

        public static void ApplySettings(FeatureDetectionSettings settings)
        {
            ScreenStateThread.Instace.FPS = settings.ScreenImageUpdateFPS;
            FeatureDetectionThread.Instace.FPS = settings.ImageDetectionFPS;
            FeatureDetectionThread.Instace.DetectionMode = settings.DetectionMode;
        }
    }
}
