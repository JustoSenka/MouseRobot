using RobotRuntime.Settings;
using System;
using System.Drawing;

namespace Robot.Settings
{
    [Serializable]
    public class DesignSettings : BaseSettings
    {
        public Font DefaultWindowFont { get; set; } = new Font(FontFamily.GenericSansSerif, 9.75F, FontStyle.Regular);

        public Font TestRunnerWindowFont { get; set; } = new Font(FontFamily.GenericSansSerif, 9.75F, FontStyle.Regular);
        public Font AssetsWindowFont { get; set; } = new Font(FontFamily.GenericSansSerif, 9.75F, FontStyle.Regular);

        public Font HierarchyCommandFont { get; set; } = new Font("Consolas", 11.25f, FontStyle.Regular);
        public Font HierarchyRecordingFont { get; set; } = new Font("Consolas", 11.25f, FontStyle.Regular);
    }
}
