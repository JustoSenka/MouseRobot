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

        public Keys LeftMouseDownButton { get; set; } = Keys.LButton;
        public Keys RightMouseDownButton { get; set; } = Keys.RButton;
        public Keys MiddleMouseDownButton { get; set; } = Keys.MButton;

        public int ThresholdBetweenMouseDownAndMouseUp { get; set; } = 20;

        public bool AutomaticSmoothMoveBeforeMouseDown { get; set; } = false;
        public bool AutomaticSmoothMoveBeforeMouseUp { get; set; } = true;

        public Keys ForImage { get; set; } = Keys.E;
        public Keys ForEachImage { get; set; } = Keys.R;
        public Keys FindImage { get; set; } = Keys.Q;
        public Keys CropImage { get; set; } = Keys.W;

        public int ScreenScaling { get; set; } = 100;
    }
}
