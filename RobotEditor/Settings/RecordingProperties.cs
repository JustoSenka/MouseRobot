using Robot.Settings;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using RobotEditor.Utils;
using RobotRuntime.Settings;
using RobotRuntime;

namespace RobotEditor.Settings
{
    [Serializable]
    [PropertyDesignerType(typeof(RecordingSettings))]
    public class RecordingProperties : BaseProperties
    {
        [NonSerialized]
        private RecordingSettings m_Settings;

        [Browsable(false)]
        public override string Title { get { return "Recording Settings"; } }

        public RecordingProperties(BaseSettings settings) : base(settings)
        {
            this.m_Settings = (RecordingSettings)settings;
        }

        public override void HideProperties(ref DynamicTypeDescriptor dt)
        {
            /*if (TreatMouseDownAsMouseClick)
                dt.RemoveProperty("ThresholdBetweenMouseDownAndMouseUp");*/
        }


        private const int NumOfCategories = 5;
        private const int SleepOptionsCategoryPosition = 1;
        private const int MouseMoveOptionsCategoryPosition = 2;
        private const int MouseOptionsCategoryPosition = 3;
        private const int AdditionalOptionsCategoryPosition = 4;
        private const int ImageCapturingCategoryPosition = 5;
        private const int ScreenSettingsCategoryPosition = 6;

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
        [DisplayName("Left Mouse Down Key")]
        public Keys LeftMouseDownButton
        {
            get { return m_Settings.LeftMouseDownButton; }
            set { m_Settings.LeftMouseDownButton = value; }
        }

        [SortedCategory("Mouse Options", MouseOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(Keys.RButton)]
        [DisplayName("Right Mouse Down Key")]
        public Keys RightMouseDownButton
        {
            get { return m_Settings.RightMouseDownButton; }
            set { m_Settings.RightMouseDownButton = value; }
        }

        [SortedCategory("Mouse Options", MouseOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(Keys.MButton)]
        [DisplayName("Middle Mouse Down Key")]
        public Keys MiddleMouseDownButton
        {
            get { return m_Settings.MiddleMouseDownButton; }
            set { m_Settings.MiddleMouseDownButton = value; }
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
        [DefaultValue(Keys.E)]
        [DisplayName("Perform Action For Image Position (Hold)")]
        public Keys ForImage
        {
            get { return m_Settings.ForImage; }
            set { m_Settings.ForImage = value; }
        }


        [SortedCategory("Image Capturing", ImageCapturingCategoryPosition, NumOfCategories)]
        [DefaultValue(Keys.R)]
        [DisplayName("Perform Action For Each Image (Hold)")]
        public Keys ForEachImage
        {
            get { return m_Settings.ForEachImage; }
            set { m_Settings.ForEachImage = value; }
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

        [SortedCategory("Screen Settings", ScreenSettingsCategoryPosition, NumOfCategories)]
        [DefaultValue(100)]
        [DisplayName("Windows screen scaling")]
        public int ScreenScaling
        {
            get { return m_Settings.ScreenScaling; }
            set { m_Settings.ScreenScaling = value; }
        }
    }
}
