using RobotRuntime.Abstractions;
using RobotRuntime.IO;
using RobotRuntime.Utils;
using System.IO;

namespace RobotRuntime.Settings
{
    public class RuntimeSettings : IRuntimeSettings
    {
        private readonly IDetectionManager DetectionManager;
        public RuntimeSettings(IDetectionManager DetectionManager)
        {
            this.DetectionManager = DetectionManager;
        }

        /// <summary>
        /// Will be replaced when settings per command are implemented.
        /// </summary>
        public void LoadSettingsHardcoded()
        {
            var path = Path.Combine(Paths.RoamingAppdataPath, "FeatureDetectionSettings.config");
            var settings = new JsonObjectIO().LoadObject<FeatureDetectionSettings>(path);
            ApplySettings(settings);
        }

        // TODO: Probably add read here, since runtime also needs to read settings from file somehow imo
        public void ApplySettings(FeatureDetectionSettings settings)
        {
            DetectionManager.ApplySettings(settings);
        }
    }
}
