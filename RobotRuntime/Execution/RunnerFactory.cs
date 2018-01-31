using RobotRuntime.Utils;
using System;
using System.Linq;
using RobotRuntime.Abstractions;

namespace RobotRuntime.Execution
{
    public class RunnerFactory : IRunnerFactory
    {
        public CommandRunningCallback Callback { get; set; }
        public LightScript ExecutingScript { get; set; }
        public ValueWrapper<bool> CancellingPointerPlaceholder { get; set; }

        private IAssetGuidManager AssetGuidManager;
        private IFeatureDetectionThread FeatureDetectionThread;

        public RunnerFactory(IFeatureDetectionThread FeatureDetectionThread, IAssetGuidManager AssetGuidManager)
        {
            this.AssetGuidManager = AssetGuidManager;
            this.FeatureDetectionThread = FeatureDetectionThread;
        }

        public IRunner CreateFor(Type type)
        {
            if (DoesRunnerSupportType(typeof(SimpleCommandRunner), type))
                return new SimpleCommandRunner(this, Callback);

            else if (DoesRunnerSupportType(typeof(ImageCommandRunner), type))
                return new ImageCommandRunner(this, FeatureDetectionThread, AssetGuidManager, ExecutingScript, Callback, CancellingPointerPlaceholder);

            else if (DoesRunnerSupportType(typeof(ScriptRunner), type))
                return new ScriptRunner(this, Callback, CancellingPointerPlaceholder);

            else
            {
                Logger.Log(LogType.Error, "Threre is no Runner registered that would support type: " + type);
                return new SimpleCommandRunner(this, Callback);
            }
        }

        // TODO: currently runner is created for every single command, and this is executed quite often, might be slow. Consider caching everyhing in Dictionary
        public bool DoesRunnerSupportType(Type runnerType, Type supportedType)
        {
            return runnerType.GetCustomAttributes(false).OfType<SupportedTypeAttribute>().Where(a => a.type == supportedType).Count() > 0;
        }
    }
}
