using RobotRuntime.Settings;

namespace RobotRuntime.Abstractions
{
    public interface IRuntimeSettings
    {
        void ApplySettings(FeatureDetectionSettings settings);
    }
}