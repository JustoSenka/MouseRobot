using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Graphics;
using System.Reflection;
using Unity;
using Unity.Lifetime;

namespace RobotEditor.PropertyUtils
{
    /// <summary>
    /// This class is used to pass dependencies to UITypeEditor and StringConverter of .net classes. Since I cannot control the instantiation of these classes,
    /// no other way to pass dependencies. Now it is done in a hacky way. Passed through reflection to private static field.
    /// </summary>
    [RegisterTypeToContainer(typeof(PropertyDependencyProvider), typeof(ContainerControlledLifetimeManager))]
    public class PropertyDependencyProvider
    {
        public static BindingFlags StaticNonPublic = BindingFlags.Static | BindingFlags.NonPublic;

        public PropertyDependencyProvider(IUnityContainer UnityContainer, IAssetManager AssetManager, ICommandFactory CommandFactory,
            IScriptLoader ScriptLoader, IFactoryWithCache<FeatureDetector> FeatureDetectorFactory, IFactoryWithCache<TextDetector> TextDetectorFactory)
        {
            typeof(AssetGUIDImageStringConverter).GetProperty("AssetManager", StaticNonPublic).SetValue(null, AssetManager);
            typeof(AssetGUIDImageUITypeEditor).GetProperty("AssetManager", StaticNonPublic).SetValue(null, AssetManager);
            typeof(RecordingGUIDStringConverter).GetProperty("AssetManager", StaticNonPublic).SetValue(null, AssetManager);

            typeof(DetectorNameStringConverter).GetProperty("FeatureDetectorFactory", StaticNonPublic).SetValue(null, FeatureDetectorFactory);
            typeof(DetectorNameStringConverterWithDefault).GetProperty("FeatureDetectorFactory", StaticNonPublic).SetValue(null, FeatureDetectorFactory);

            typeof(TextDetectorNameStringConverter).GetProperty("TextDetectorFactory", StaticNonPublic).SetValue(null, TextDetectorFactory);
            typeof(TextDetectorNameStringConverterWithDefault).GetProperty("TextDetectorFactory", StaticNonPublic).SetValue(null, TextDetectorFactory);

            typeof(CommandNameStringConverter).GetProperty("CommandFactory", StaticNonPublic).SetValue(null, CommandFactory);

            typeof(AppDomainExtension).GetProperty("ScriptLoader", StaticNonPublic).SetValue(null, ScriptLoader);
        }
    }
}
