﻿using RobotRuntime.Abstractions;
using RobotRuntime.IO;
using RobotRuntime.Utils;
using System.IO;
using Unity.Lifetime;

namespace RobotRuntime.Settings
{
    [RegisterTypeToContainer(typeof(IRuntimeSettings), typeof(ContainerControlledLifetimeManager))]
    public class RuntimeSettings : IRuntimeSettings
    {
        private readonly IDetectionManager DetectionManager;
        private readonly ITextDetectionManager TextDetectionManager;
        public RuntimeSettings(IFeatureDetectionManager DetectionManager, ITextDetectionManager TextDetectionManager)
        {
            this.DetectionManager = DetectionManager;
            this.TextDetectionManager = TextDetectionManager;
        }

        /// <summary>
        /// Will be replaced when settings per command are implemented.
        /// </summary>
        public void LoadSettingsHardcoded()
        {
            var path = Path.Combine(Paths.RoamingAppdataPath, "FeatureDetectionSettings.config");
            var settings = new JsonObjectIO().LoadObject<DetectionSettings>(path);

            // Settings might be null if no project was opened on this machine ever, just give default settings then
            if (settings == null)
                settings = new DetectionSettings();

            ApplySettings(settings);
        }

        // TODO: Probably add read here, since runtime also needs to read settings from file somehow imo
        public void ApplySettings(DetectionSettings settings)
        {
            DetectionManager.ApplySettings(settings);
            TextDetectionManager.ApplySettings(settings);
        }
    }
}
