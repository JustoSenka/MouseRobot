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
        public IEnumerable<BaseSettings> Settings { get { return TypeCollector.AllObjects; } }

        public event Action SettingsRestored;

        private ObjectIO m_Serializer = new JsonObjectIO();

        private ILogger Logger;
        private IScriptLoader ScriptLoader;
        private IUnityContainer Container;
        private ICustomTypeObjectCollector<BaseSettings> TypeCollector;
        public SettingsManager(IUnityContainer Container, IScriptLoader ScriptLoader, ILogger Logger, IProjectManager ProjectManager, ICustomTypeObjectCollector<BaseSettings> TypeCollector)
        {
            this.Container = Container;
            this.ScriptLoader = ScriptLoader;
            this.Logger = Logger;
            this.TypeCollector = TypeCollector;

            ScriptLoader.UserDomainReloading += OnDomainReloading;

            CreateIfNotExist(Paths.RoamingAppdataPath);
            CreateIfNotExist(Paths.LocalAppdataPath);
            
            TypeCollector.NewTypesAppeared += () => RestoreSettings();
            ProjectManager.NewProjectOpened += (str) => RestoreSettings();
        }

        private void OnDomainReloading()
        {
            // TODO: Optimize, we only need user settings to be saved
            SaveSettings();
        }

        public void RestoreDefaults()
        {
            TypeCollector.RestoreDefaultObjects();
            SettingsRestored?.Invoke();
        }

        ~SettingsManager()
        {
            SaveSettings();
        }

        public void SaveSettings()
        {
            foreach (var s in TypeCollector.AllObjects)
                WriteToSettingFile(s);
        }

        public void RestoreSettings()
        {
            for (int i = 0; i < TypeCollector.AllObjects.Count(); i++)
            {
                // Will not work
                // Need a mechanism to replace all props or to replace whole objects
                // TypeCollector.AllObjects.ElementAt(i) = RestoreSettingFromFile(TypeCollector.AllObjects.ElementAt(i));
            }

            SettingsRestored?.Invoke();
        }

        public T GetSettings<T>() where T : BaseSettings
        {
            return (T)GetSettingsFromType(typeof(T));
        }

        public BaseSettings GetSettingsFromType(Type type)
        {
            return TypeCollector.AllObjects.FirstOrDefault(s => s.GetType() == type);
        }

        public BaseSettings GetSettingsFromName(string fullTypeName)
        {
            // DO-DOMAIN will not work if types are in different domain
            return TypeCollector.AllObjects.FirstOrDefault(s => s.GetType().FullName == fullTypeName);
        }

        private void WriteToSettingFile<T>(T settings) where T : BaseSettings
        {
            string filePath = RoamingAppdataPathFromType(settings);
            m_Serializer.SaveObject(filePath, settings);
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

                MethodInfo method = m_Serializer.GetType().GetMethod("LoadObject");
                MethodInfo generic = method.MakeGenericMethod(settings.GetType());
                var newSettings = generic.Invoke(m_Serializer, new[] { filePath });

                // If deserializing from file fails, restore defaults
                if (newSettings == null)
                    newSettings = (T)Container.Resolve(settings.GetType());

                return (T)newSettings;
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
