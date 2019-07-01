using RobotRuntime.Settings;

namespace RobotRuntime.Abstractions
{
    public interface IRuntimeSettings
    {
        void LoadSettingsHardcoded();
        void ApplySettings(DetectionSettings settings);
    }
}