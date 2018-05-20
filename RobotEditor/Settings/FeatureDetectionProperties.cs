using RobotEditor.Scripts.Utils;
using RobotEditor.Utils;
using RobotRuntime.Settings;
using System;
using System.ComponentModel;

namespace RobotEditor.Scripts
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

        public override void OnPropertiesModified()
        {
            //RuntimeSettings.ApplySettings(SettingsManager.FeatureDetectionSettings);
        }

        private const int NumOfCategories = 2;
        private const int DetectionModeCategoryPosition = 1;
        private const int ThreadFramerateCategoryPosition = 2;

        [SortedCategory("Detection Mode", DetectionModeCategoryPosition, NumOfCategories)]
        [DefaultValue("SURF")]
        [DisplayName("DetectionMode")]
        [TypeConverter(typeof(DetectorNameStringConverter))]
        public string DetectionMode
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
