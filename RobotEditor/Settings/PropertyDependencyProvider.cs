using Robot.Abstractions;
using RobotEditor.Scripts.Utils;
using RobotRuntime.Abstractions;
using System.Reflection;
using Unity;

namespace RobotEditor.Settings
{
    /// <summary>
    /// This class is used to pass dependencies to UITypeEditor and StringConverter of .net classes. Since I cannot control the instantiation of these classes,
    /// no other way to pass dependencies. Now it is done in a hacky way. Passed through reflection to private static field.
    /// </summary>
    public class PropertyDependencyProvider
    {
        public PropertyDependencyProvider(IUnityContainer UnityContainer, IAssetManager AssetManager, IFeatureDetectorFactory FeatureDetectorFactory)
        {
            typeof(AssetGUIDImageStringConverter).GetProperty("AssetManager", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, AssetManager);
            typeof(AssetGUIDImageUITypeEditor).GetProperty("AssetManager", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, AssetManager);

            typeof(DetectorNameStringConverter).GetProperty("FeatureDetectorFactory", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, FeatureDetectorFactory);
        }
    }
}
