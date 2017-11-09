using Robot.Settings;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using RobotEditor.Utils;
using RobotRuntime.Settings;

namespace RobotEditor.Settings
{
    [Serializable]
    public class RecordingProperties : BaseProperties
    {
        [NonSerialized]
        private RecordingSettings m_Settings;

        [Browsable(false)]
        public override string Title { get { return "Recording Settings"; } }

        public RecordingProperties(BaseSettings settings)
        {
            this.m_Settings = (RecordingSettings)settings;
        }

        public override void HideProperties(DynamicTypeDescriptor dt)
        {
            if (TreatMouseDownAsMouseClick)
                dt.RemoveProperty("ThresholdBetweenMouseDownAndMouseUp");
        }


        private const int NumOfCategories = 5;
        private const int SleepOptionsCategoryPosition = 1;
        private const int MouseMoveOptionsCategoryPosition = 2;
        private const int MouseOptionsCategoryPosition = 3;
        private const int AdditionalOptionsCategoryPosition = 4;
        private const int ImageCapturingCategoryPosition = 5;

        [SortedCategory("Sleep Options", SleepOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(Keys.S)]
        [DisplayName("Sleep Key")]
        public Keys SleepKey
        {
            get { return m_Settings.SleepKey; }
            set { m_Settings.SleepKey = value; }
        }

        [SortedCategory("Sleep Options", SleepOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(Keys.D)]
        [DisplayName("Default Sleep Key")]
        public Keys DefaultSleepKey
        {
            get { return m_Settings.DefaultSleepKey; }
            set { m_Settings.DefaultSleepKey = value; }
        }

        [SortedCategory("Sleep Options", SleepOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(1000)]
        [DisplayName("Default Sleep Time")]
        public int DefaultSleepTime
        {
            get { return m_Settings.DefaultSleepTime; }
            set { m_Settings.DefaultSleepTime = value; }
        }

        [SortedCategory("Mouse Move Options", MouseMoveOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(Keys.F)]
        [DisplayName("Smooth Mouse Move Key")]
        public Keys SmoothMouseMoveKey
        {
            get { return m_Settings.SmoothMouseMoveKey; }
            set { m_Settings.SmoothMouseMoveKey = value; }
        }

        [SortedCategory("Mouse Move Options", MouseMoveOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(100)]
        [DisplayName("Smooth Move Length in Time")]
        public int SmoothMoveLengthInTime
        {
            get { return m_Settings.SmoothMoveLengthInTime; }
            set { m_Settings.SmoothMoveLengthInTime = value; }
        }

        [SortedCategory("Mouse Options", MouseOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(Keys.LButton)]
        [DisplayName("Mouse Down Key")]
        public Keys MouseDownButton
        {
            get { return m_Settings.MouseDownButton; }
            set { m_Settings.MouseDownButton = value; }
        }

        [SortedCategory("Mouse Options", MouseOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(false)]
        [DisplayName("Treat Mouse Down as Mouse Click")]
        public bool TreatMouseDownAsMouseClick
        {
            get { return m_Settings.TreatMouseDownAsMouseClick; }
            set { m_Settings.TreatMouseDownAsMouseClick = value; }
        }

        [SortedCategory("Mouse Options", MouseOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(20)]
        [DisplayName("Threshold Between Mouse Down and Mouse Up")]
        public int ThresholdBetweenMouseDownAndMouseUp
        {
            get { return m_Settings.ThresholdBetweenMouseDownAndMouseUp; }
            set { m_Settings.ThresholdBetweenMouseDownAndMouseUp = value; }
        }

        [SortedCategory("Additional Options", AdditionalOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(false)]
        [DisplayName("Automatic Smooth Move Before Mouse Down")]
        public bool AutomaticSmoothMoveBeforeMouseDown
        {
            get { return m_Settings.AutomaticSmoothMoveBeforeMouseDown; }
            set { m_Settings.AutomaticSmoothMoveBeforeMouseDown = value; }
        }

        [SortedCategory("Additional Options", AdditionalOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(true)]
        [DisplayName("Automatic Smooth Move Before Mouse Up")]
        public bool AutomaticSmoothMoveBeforeMouseUp
        {
            get { return m_Settings.AutomaticSmoothMoveBeforeMouseUp; }
            set { m_Settings.AutomaticSmoothMoveBeforeMouseUp = value; }
        }

        [SortedCategory("Image Capturing", ImageCapturingCategoryPosition, NumOfCategories)]
        [DefaultValue(Keys.Shift)]
        [DisplayName("Perform Action On Image (Hold)")]
        public Keys PerformActionOnImage
        {
            get { return m_Settings.PerformActionOnImage; }
            set { m_Settings.PerformActionOnImage = value; }
        }

        [SortedCategory("Image Capturing", ImageCapturingCategoryPosition, NumOfCategories)]
        [DefaultValue(Keys.Q)]
        [DisplayName("Find Hovered Image Reference")]
        public Keys FindImage
        {
            get { return m_Settings.FindImage; }
            set { m_Settings.FindImage = value; }
        }

        [SortedCategory("Image Capturing", ImageCapturingCategoryPosition, NumOfCategories)]
        [DefaultValue(Keys.W)]
        [DisplayName("Start Cropping Image")]
        public Keys CropImage
        {
            get { return m_Settings.CropImage; }
            set { m_Settings.CropImage = value; }
        }
    }
}
