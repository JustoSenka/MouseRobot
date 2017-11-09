using RobotRuntime.Settings;
using System;
using System.Windows.Forms;

namespace Robot.Settings
{
    [Serializable]
    public class RecordingSettings : BaseSettings
    {
        public Keys SleepKey { get; set; } = Keys.S;
        public Keys DefaultSleepKey { get; set; } = Keys.D;
        public int DefaultSleepTime { get; set; } = 1000;

        public Keys SmoothMouseMoveKey { get; set; } = Keys.F;
        public int SmoothMoveLengthInTime { get; set; } = 100;

        public Keys MouseDownButton { get; set; } = Keys.LButton;
        public bool TreatMouseDownAsMouseClick { get; set; } = false;
        public int ThresholdBetweenMouseDownAndMouseUp { get; set; } = 20;

        public bool AutomaticSmoothMoveBeforeMouseDown { get; set; } = false;
        public bool AutomaticSmoothMoveBeforeMouseUp { get; set; } = true;

        public Keys PerformActionOnImage { get; set; } = Keys.Shift;
        public Keys FindImage { get; set; } = Keys.Q;
        public Keys CropImage { get; set; } = Keys.W;
    }
}
