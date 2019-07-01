using System;

namespace RobotRuntime.Settings
{
    [Serializable]
    public class DetectionSettings : BaseSettings
    {
        public string ImageDetectionMode { get; set; } = DetectorNamesHardcoded.SURF;

        public string TextDetectionMode { get; set; } = DetectorNamesHardcoded.Tesseract;

        public int ScreenImageUpdateFPS { get; set; } = 10;
        public int ImageDetectionFPS { get; set; } = 10;
    }

    public static class DetectorNamesHardcoded
    {
        public const string Default = "Default";
        public const string PixelPerfect = "PixelPerfect";
        public const string SURF = "SURF";
        public const string Template = "Template";
        public const string Tesseract = "Tesseract";
    }
}
