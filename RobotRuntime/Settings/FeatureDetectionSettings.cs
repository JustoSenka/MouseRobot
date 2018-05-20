using System;

namespace RobotRuntime.Settings
{
    [Serializable]
    public class FeatureDetectionSettings : BaseSettings
    {
        public string DetectionMode { get; set; } = DetectorNamesHardcoded.SURF;

        public int ScreenImageUpdateFPS { get; set; } = 10;
        public int ImageDetectionFPS { get; set; } = 10;
    }

    public static class DetectorNamesHardcoded
    {
        public static string PixelPerfect { get { return "PixelPerfect"; } }
        public static string SURF { get { return "SURF"; } }
        public static string Template { get { return "Template"; } }
    }
}
