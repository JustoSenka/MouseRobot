using Robot.Settings;
using Robot.Utils;
using RobotEditor.Utils;
using System;
using System.ComponentModel;

namespace RobotEditor.Settings
{
    public class FeatureDetectionProperties : BaseProperties
    {
        [NonSerialized]
        private FeatureDetectionSettings m_Settings;

        [Browsable(false)]
        public override string Title { get { return "Image Detection Settings"; } }

        public FeatureDetectionProperties(BaseSettings settings)
        {
            this.m_Settings = (FeatureDetectionSettings)settings;
        }

        public override void HideProperties(DynamicTypeDescriptor dt)
        {

        }

        private const int NumOfCategories = 2;
        private const int DetectionModeCategoryPosition = 1;
        private const int ThreadFramerateCategoryPosition = 2;

        [SortedCategory("Detection Mode", DetectionModeCategoryPosition, NumOfCategories)]
        [DefaultValue(Robot.Settings.DetectionMode.FeatureSURF)]
        [DisplayName("DetectionMode")]
        public DetectionMode DetectionMode
        {
            get { return m_Settings.DetectionMode; }
            set { m_Settings.DetectionMode = value; }
        }

        [SortedCategory("Thread Framerate", ThreadFramerateCategoryPosition, NumOfCategories)]
        [DefaultValue(10)]
        [DisplayName("Screen Image Update Framerate")]
        public int ScreenImageUpdateFPS
        {
            get { return m_Settings.ScreenImageUpdateFPS; }
            set { m_Settings.ScreenImageUpdateFPS = value; }
        }

        [SortedCategory("Thread Framerate", ThreadFramerateCategoryPosition, NumOfCategories)]
        [DefaultValue(10)]
        [DisplayName("Image Detection Framerate")]
        public int ImageDetectionFPS
        {
            get { return m_Settings.ImageDetectionFPS; }
            set { m_Settings.ImageDetectionFPS = value; }
        }
    }
}
