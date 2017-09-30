using System.Windows.Forms;

namespace Robot
{
    interface ICommandManagerProperties
    {
        Keys SleepKey { get; set; }
        Keys DefaultSleepKey { get; set; }
        int DefaultSleepTime { get; set; }

        Keys SmoothMouseMoveKey { get; set; }
        int SmoothMoveLengthInTime { get; set; }

        bool AutomaticSmoothMoveBetweenClicks { get; set; }
        bool TreatMouseDownAsMouseClick { get; set; }
        int ThresholdBetweenMouseDownAndMouseUp { get; set; }
    }
}
