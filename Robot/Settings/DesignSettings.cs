using RobotRuntime.Settings;
using System;
using System.Drawing;

namespace Robot.Settings
{
    [Serializable]
    public class DesignSettings : BaseSettings
    {
        public Font DefaultWindowFont { get; set; } = new Font(FontFamily.GenericSansSerif, 8.25F, FontStyle.Regular);
        public Font HierarchyWindowsFont { get; set; } = new Font("Consolas", 11f, FontStyle.Regular);
        public Font TestRunnerWindowFont { get; set; } = new Font(FontFamily.GenericSansSerif, 8.25F, FontStyle.Regular);
        public Font AssetsWindowFont { get; set; } = new Font(FontFamily.GenericSansSerif, 8.25F, FontStyle.Regular);
    }
}
