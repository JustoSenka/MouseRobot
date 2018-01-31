using Robot.Abstractions;
using RobotRuntime.IO;
using RobotRuntime.Settings;
using RobotRuntime.Utils;
using System;
using System.IO;
using System.Linq;

namespace Robot.Settings
{
    public class SettingsManager : ISettingsManager
    {
        public RecordingSettings RecordingSettings { get; private set; }
        public FeatureDetectionSettings FeatureDetectionSettings { get; private set; }

        public SettingsManager()
        {
            CreateIfNotExist(Paths.RoamingAppdataPath);
            CreateIfNotExist(Paths.LocalAppdataPath);

            RestoreDefaults();
            RestoreSettings();
        }

        ~SettingsManager()
        {
            SaveSettings();
        }

        public void SaveSettings()
        {
            WriteToSettingFile(RecordingSettings);
            WriteToSettingFile(FeatureDetectionSettings);
        }

        public void RestoreSettings()
        {
            RecordingSettings = RestoreSettingFromFile(RecordingSettings);
            FeatureDetectionSettings = RestoreSettingFromFile(FeatureDetectionSettings);
        }

        public void RestoreDefaults()
        {
            RecordingSettings = new RecordingSettings();
            FeatureDetectionSettings = new FeatureDetectionSettings();
        }

        private void WriteToSettingFile<T>(T settings) where T : BaseSettings
        {
            string filePath = RoamingAppdataPathFromType(settings);
            new YamlObjectIO().SaveObject(filePath, settings);
        }

        private T RestoreSettingFromFile<T>(T settings) where T : BaseSettings
        {
            string filePath = RoamingAppdataPathFromType(settings);
            if (File.Exists(filePath))
            {
                var newSettings = new YamlObjectIO().LoadObject<T>(filePath);
                return newSettings ?? settings;
            }
            else
                return settings;
        }

        private string RoamingAppdataPathFromType<T>(T settings) where T : BaseSettings
        {
            string fileName = FileNameFromType(settings);
            var filePath = Path.Combine(Paths.RoamingAppdataPath, fileName);
            return filePath;
        }

        private static string FileNameFromType<T>(T type) where T : BaseSettings
        {
            return type.ToString().Split('.').Last() + ".config";
        }

        private static void CreateIfNotExist(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
