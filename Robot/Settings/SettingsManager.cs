using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.IO;
using RobotRuntime.Settings;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity;

namespace Robot.Settings
{
    public class SettingsManager : ISettingsManager
    {
        public IEnumerable<BaseSettings> Settings { get { return m_Settings; } }

        public event Action SettingsRestored;

        private BaseSettings[] m_NativeSettings;
        private BaseSettings[] m_UserSettings;
        private BaseSettings[] m_Settings;

        private ILogger Logger;
        private IPluginLoader PluginLoader;
        private IUnityContainer Container;
        public SettingsManager(IUnityContainer Container, IPluginLoader PluginLoader, ILogger Logger)
        {
            this.Container = Container;
            this.PluginLoader = PluginLoader;
            this.Logger = Logger;

            PluginLoader.UserDomainReloaded += OnDomainReloaded;
            PluginLoader.UserDomainReloading += OnDomainReloading;

            CreateIfNotExist(Paths.RoamingAppdataPath);
            CreateIfNotExist(Paths.LocalAppdataPath);

            CollectDefaultNativeSettings();
            CollectDefaultUserSettings();

            RestoreSettings();
        }

        private void OnDomainReloading()
        {
            // TODO: Optimize, we only need user settings to be saved
            SaveSettings();
        }

        private void OnDomainReloaded()
        {
            // TODO: Optimize, we only need user settings to be restored
            CollectDefaultUserSettings();
            RestoreSettings();
        }

        public void RestoreDefaults()
        {
            CollectDefaultNativeSettings();
            CollectDefaultUserSettings();
            SettingsRestored?.Invoke();
        }

        private void CollectDefaultNativeSettings()
        {
            var types = AppDomain.CurrentDomain.GetNativeAssemblies().GetAllTypesWhichImplementInterface(typeof(BaseSettings));
            m_NativeSettings = types.Select(t => Container.Resolve(t)).Cast<BaseSettings>().ToArray();
        }

        private void CollectDefaultUserSettings()
        {
            // DO-DOMAIN: This will not work if assemblies are in different domain
            var types = PluginLoader.IterateUserAssemblies(a => a).GetAllTypesWhichImplementInterface(typeof(BaseSettings));
            m_UserSettings = types.TryResolveTypes(Container, Logger).Cast<BaseSettings>().ToArray();

            m_Settings = m_NativeSettings.Concat(m_UserSettings).ToArray();
        }

        ~SettingsManager()
        {
            SaveSettings();
        }

        public void SaveSettings()
        {
            foreach (var s in m_Settings)
                WriteToSettingFile(s);
        }

        public void RestoreSettings()
        {
            for(int i = 0; i < m_Settings.Length; i++)
                m_Settings[i] = RestoreSettingFromFile(m_Settings[i]);

            SettingsRestored?.Invoke();
        }

        public T GetSettings<T>() where T : BaseSettings
        {
            return (T) GetSettingsFromType(typeof(T));
        }

        public BaseSettings GetSettingsFromType(Type type)
        {
            return m_Settings.FirstOrDefault(s => s.GetType() == type);
        }

        public BaseSettings GetSettingsFromName(string fullTypeName)
        {
            // DO-DOMAIN will not work if types are in different domain
            return m_Settings.FirstOrDefault(s => s.GetType().FullName == fullTypeName);
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
                //var newSettings = new YamlObjectIO().LoadObject<T>(filePath);
                // This will not work, because T is always BaseSettings and not the actual type
                // so deserializer will look for BaseSettings.
                // Using reflection it is possible to Invoke deserializer with T as actual type and not base type.

                MethodInfo method = typeof(YamlObjectIO).GetMethod("LoadObject");
                MethodInfo generic = method.MakeGenericMethod(settings.GetType());
                var newSettings = generic.Invoke(new YamlObjectIO(), new[] { filePath });

                // If deserializing from file fails, restore defaults
                if (newSettings == null)
                    newSettings = (T) Container.Resolve(settings.GetType());

                return (T) newSettings;
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
