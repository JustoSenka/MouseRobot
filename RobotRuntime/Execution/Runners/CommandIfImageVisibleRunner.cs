using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Tests;
using System.Drawing;
using System.Linq;

namespace RobotRuntime.Execution
{
    public class CommandIfImageVisibleRunner : IRunner
    {
        public TestData TestData { set; get; }

        private IRuntimeAssetManager RuntimeAssetManager;
        private IFeatureDetectionThread FeatureDetectionThread;
        private ILogger Logger;
        public CommandIfImageVisibleRunner(IFeatureDetectionThread FeatureDetectionThread, IRuntimeAssetManager RuntimeAssetManager, ILogger Logger)
        {
            this.RuntimeAssetManager = RuntimeAssetManager;
            this.Logger = Logger;
            this.FeatureDetectionThread = FeatureDetectionThread;
        }

        public void Run(IRunnable runnable)
        {
            if (!TestData.RunnerFactory.DoesRunnerSupportType(this.GetType(), runnable.GetType()))
            {
                Logger.Logi(LogType.Error, "This runner '" + this + "' is not compatible with this type: '" + runnable.GetType());
                return;
            }

            var command = runnable as CommandIfImageVisible;
            var commandNode = TestData.TestFixture.Commands.FirstOrDefault(n => n.value == command);

            if (Logger.AssertIf(command.Asset.IsDefault(), "Command does not have valid image referenced: " + command.ToString()))
                return;

            var image = RuntimeAssetManager.GetAsset<Bitmap>(command.Asset);
            if (image == null)
            {
                TestData.ShouldFailTest = true;
                return;
            }

            TestData.InvokeCallback(commandNode.value.Guid);
            var points = FeatureDetectionThread.FindImageSync(image, command.Timeout);

            // Image was not found
            if (points == null || points.Length == 0)
            {
                if (command.ExpectTrue)
                    return;

                foreach (var childNode in commandNode)
                {
                    if (TestData.ShouldCancelRun)
                        return;

                    var runner = TestData.RunnerFactory.GetFor(childNode.value.GetType());
                    runner.Run(childNode.value);
                }
            }
            else // Image was found
            {
                if (!command.ExpectTrue)
                    return;

                foreach (var p in points)
                {
                    command.Run(TestData);

                    foreach (var childNode in commandNode)
                    {
                        if (TestData.ShouldCancelRun)
                            return;

                        OverrideCommandPropertiesIfExist(childNode.value, p.X, "X");
                        OverrideCommandPropertiesIfExist(childNode.value, p.Y, "Y");

                        var runner = TestData.RunnerFactory.GetFor(childNode.value.GetType());
                        runner.Run(childNode.value);
                    }
                }
            }
        }

        private static void OverrideCommandPropertiesIfExist(Command command, object value, string prop)
        {
            var destProp = command.GetType().GetProperty(prop);

            if (destProp != null)
                destProp.SetValue(command, value);
        }
    }
}
