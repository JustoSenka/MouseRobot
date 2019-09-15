using RobotRuntime.Settings;
using System;

namespace Robot.Settings
{
    [Serializable]
    public class DesignSettings : BaseSettings
    {
        public string Font { get; set; } = "Consolas";
    }
}
