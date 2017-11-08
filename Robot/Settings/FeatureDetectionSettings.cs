using System;

namespace Robot.Settings
{
    [Serializable]
    public class FeatureDetectionSettings : BaseSettings
    {
        public DetectionMode DetectionMode { get; set; } = DetectionMode.FeatureSURF;

        public int ScreenImageUpdateFPS { get; set; } = 10;
        public int ImageDetectionFPS { get; set; } = 10;
    }

    [Serializable]
    public enum DetectionMode
    {
        PixelPerfect, Template, FeatureSURF, FeatureFAST
    }
}
