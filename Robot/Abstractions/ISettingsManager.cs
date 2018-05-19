using Robot.Settings;
using RobotRuntime.Settings;
using System;
using System.Collections.Generic;

namespace Robot.Abstractions
{
    public interface ISettingsManager
    {
        IEnumerable<BaseSettings> Settings { get; }

        T GetSettings<T>() where T : BaseSettings;
        BaseSettings GetSettingsFromType(Type type);
        BaseSettings GetSettingsFromName(string fullTypeName);

        void RestoreDefaults();
        void RestoreSettings();
        void SaveSettings();
    }
}