using RobotEditor.PropertyUtils;
using RobotEditor.Utils;
using RobotRuntime.Settings;
using RobotRuntime;
using System;
using System.ComponentModel;

namespace RobotEditor.Settings
{
    [Serializable]
    [PropertyDesignerType(typeof(DetectionSettings))]
    public class DetectionProperties : BaseProperties
    {
        [NonSerialized]
        private DetectionSettings m_Settings;

        [Browsable(false)]
        public override string Title { get { return "Detection Settings"; } }

        public DetectionProperties(BaseSettings settings) : base(settings)
        {
            this.m_Settings = (DetectionSettings)settings;
        }

        public override void HideProperties(ref DynamicTypeDescriptor dt)
        {

        }

        public override void OnPropertiesModified()
        {
            // RuntimeSettings.ApplySettings(SettingsManager.FeatureDetectionSettings);
        }

        private const int NumOfCategories = 3;
        private const int DetectionModeCategoryPosition = 1;
        private const int TextDetectionModeCategoryPosition = 2;
        private const int ThreadFramerateCategoryPosition = 3;

        [SortedCategory("Image Detection Mode", DetectionModeCategoryPosition, NumOfCategories)]
        [DefaultValue(DetectorNamesHardcoded.SURF)]
        [DisplayName("ImageDetectionMode")]
        [TypeConverter(typeof(DetectorNameStringConverter))]
        public string ImageDetectionMode
        {
            get { return m_Settings.ImageDetectionMode; }
            set { m_Settings.ImageDetectionMode  = value; }
        }

        [SortedCategory("Text Detection Mode", TextDetectionModeCategoryPosition, NumOfCategories)]
        [DefaultValue(DetectorNamesHardcoded.Tesseract)]
        [DisplayName("TextDetectionMode")]
        [TypeConverter(typeof(TextDetectorNameStringConverter))]
        public string TextDetectionMode
        {
            get { return m_Settings.TextDetectionMode; }
            set { m_Settings.TextDetectionMode = value; }
        }

        [SortedCategory("Visualization Thread Framerate", ThreadFramerateCategoryPosition, NumOfCategories)]
        [DefaultValue(10)]
        [DisplayName("Screen Image Update Framerate")]
        public int ScreenImageUpdateFPS
        {
            get { return m_Settings.ScreenImageUpdateFPS; }
            set { m_Settings.ScreenImageUpdateFPS = value; }
        }

        [SortedCategory("Visualization Thread Framerate", ThreadFramerateCategoryPosition, NumOfCategories)]
        [DefaultValue(10)]
        [DisplayName("Image Detection Framerate")]
        public int ImageDetectionFPS
        {
            get { return m_Settings.ImageDetectionFPS; }
            set { m_Settings.ImageDetectionFPS = value; }
        }
    }
}
