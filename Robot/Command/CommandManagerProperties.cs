using Robot.Utils;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Robot
{
    [Serializable]
    public class CommandManagerProperties
    {
        private const int NumOfCategories = 4;
        private const int SleepOptionsCategoryPosition = 1;
        private const int MouseMoveOptionsCategoryPosition = 2;
        private const int MouseOptionsCategoryPosition = 3;
        private const int AdditionalOptionsCategoryPosition = 4;

        private Keys m_SleepKey = Keys.S;
        private Keys m_DefaultSleepKey = Keys.D;
        private int m_DefaultSleepTime = 1000;

        private Keys m_SmoothMouseMoveKey = Keys.F;
        private int m_SmoothMoveLengthInTime = 100;

        private Keys m_MouseDownButton = Keys.LButton;
        private bool m_TreatMouseDownAsMouseClick = false;
        private int m_ThresholdBetweenMouseDownAndMouseUp = 20;

        private bool m_AutomaticSmoothMoveBeforeMouseDown = false;
        private bool m_AutomaticSmoothMoveBeforeMouseUp = true;

        [SortedCategory("Sleep Options", SleepOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(Keys.S)]
        [DisplayName("Sleep Key")]
        public Keys SleepKey
        {
            get { return m_SleepKey; }
            set { m_SleepKey = value; }
        }

        [SortedCategory("Sleep Options", SleepOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(Keys.D)]
        [DisplayName("Default Sleep Key")]
        public Keys DefaultSleepKey
        {
            get { return m_DefaultSleepKey; }
            set { m_DefaultSleepKey = value; }
        }

        [SortedCategory("Sleep Options", SleepOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(1000)]
        [DisplayName("Default Sleep Time")]
        public int DefaultSleepTime
        {
            get { return m_DefaultSleepTime; }
            set { m_DefaultSleepTime = value; }
        }

        [SortedCategory("Mouse Move Options", MouseMoveOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(Keys.F)]
        [DisplayName("Smooth Mouse Move Key")]
        public Keys SmoothMouseMoveKey
        {
            get { return m_SmoothMouseMoveKey; }
            set { m_SmoothMouseMoveKey = value; }
        }

        [SortedCategory("Mouse Move Options", MouseMoveOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(100)]
        [DisplayName("Smooth Move Length in Time")]
        public int SmoothMoveLengthInTime
        {
            get { return m_SmoothMoveLengthInTime; }
            set { m_SmoothMoveLengthInTime = value; }
        }

        [SortedCategory("Mouse Options", MouseOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(Keys.LButton)]
        [DisplayName("Mouse Down Key")]
        public Keys MouseDownButton
        {
            get { return m_MouseDownButton; }
            set { m_MouseDownButton = value; }
        }

        [SortedCategory("Mouse Options", MouseOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(false)]
        [DisplayName("Treat Mouse Down as Mouse Click")]
        public bool TreatMouseDownAsMouseClick
        {
            get { return m_TreatMouseDownAsMouseClick; }
            set { m_TreatMouseDownAsMouseClick = value; }
        }

        [SortedCategory("Mouse Options", MouseOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(20)]
        [DisplayName("Threshold Between Mouse Down and Mouse Up")]
        public int ThresholdBetweenMouseDownAndMouseUp
        {
            get { return m_ThresholdBetweenMouseDownAndMouseUp; }
            set { m_ThresholdBetweenMouseDownAndMouseUp = value; }
        }

        [SortedCategory("Additional Options", AdditionalOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(false)]
        [DisplayName("Automatic Smooth Move Before Mouse Down")]
        public bool AutomaticSmoothMoveBeforeMouseDown
        {
            get { return m_AutomaticSmoothMoveBeforeMouseDown; }
            set { m_AutomaticSmoothMoveBeforeMouseDown = value; }
        }

        [SortedCategory("Additional Options", AdditionalOptionsCategoryPosition, NumOfCategories)]
        [DefaultValue(true)]
        [DisplayName("Automatic Smooth Move Before Mouse Up")]
        public bool AutomaticSmoothMoveBeforeMouseUp
        {
            get { return m_AutomaticSmoothMoveBeforeMouseUp; }
            set { m_AutomaticSmoothMoveBeforeMouseUp = value; }
        }

    }
}
