using Robot.Settings;
using RobotRuntime.Settings;

namespace Robot.Abstractions
{
    public interface ISettingsManager
    {
        FeatureDetectionSettings FeatureDetectionSettings { get; }
        RecordingSettings RecordingSettings { get; }

        void RestoreDefaults();
        void RestoreSettings();
        void SaveSettings();
    }
}