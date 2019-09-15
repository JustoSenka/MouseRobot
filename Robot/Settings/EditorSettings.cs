using RobotRuntime.Settings;
using System;
using System.Windows.Forms;

namespace Robot.Settings
{
    [Serializable]
    public class EditorSettings : BaseSettings
    {
        public WindowState PlayingAction { get; set; } = WindowState.DoNothing;
        public WindowState RecordingAction { get; set; } = WindowState.DoNothing;
    }

    public enum WindowState
    {
        DoNothing = 0, Windowed = 1, Minimize = 2
    }
}
