using RobotEditor.Settings;
using RobotRuntime.Settings;
using System;

namespace RobotEditor.Abstractions
{
    public interface IPropertiesWindow
    {
        event Action<BaseProperties> PropertiesModified;

        void UpdateCurrentProperties();

        void ShowProperties<T>(T properties) where T : BaseProperties;
        void ShowSettings<T>(T settings) where T : BaseSettings;
    }
}