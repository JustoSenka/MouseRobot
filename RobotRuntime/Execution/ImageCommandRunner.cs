using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Graphics;
using RobotRuntime.Tests;
using RobotRuntime.Utils.Win32;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace RobotRuntime.Execution
{
    public class ImageCommandRunner : IRunner
    {
        public TestData TestData { set; get; }

        private IRuntimeAssetManager RuntimeAssetManager;
        private IFeatureDetectionThread FeatureDetectionThread;
        private ILogger Logger;
        public ImageCommandRunner(IFeatureDetectionThread FeatureDetectionThread, IRuntimeAssetManager RuntimeAssetManager, ILogger Logger)
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

            var command = runnable as Command;
            var commandNode = TestData.TestFixture.Commands.FirstOrDefault(node => node.value == command);
            RunCommand(commandNode);
        }

        private void RunCommand(TreeNode<Command> node)
        {
            GetImageAndTimeout(node, out Guid imageGuid, out int timeout);

            if (Logger.AssertIf(imageGuid.IsDefault(), "Command does not have valid image referenced: " + node.ToString()))
                return;

            var image = RuntimeAssetManager.GetAsset<Bitmap>(imageGuid);
            if (image == null)
            {
                TestData.ShouldFailTest = true;
                return;
            }

            var points = FeatureDetectionThread.FindImageSync(image, timeout);
            if (points == null || points.Length == 0)
            {
                TestData.ShouldFailTest = true;
                return;
            }

            foreach (var p in points)
            {
                TestData.CommandRunningCallback?.Invoke(node.value);
                node.value.Run(TestData);

                foreach (var childNode in node)
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

        private static void GetImageAndTimeout(TreeNode<Command> node, out Guid image, out int timeout)
        {
            var command = node.value;
            if (command is CommandForImage)
            {
                var c = (CommandForImage)command;
                image = c.Asset;
                timeout = c.Timeout;
            }
            else if (command is CommandForeachImage)
            {
                var c = (CommandForeachImage)command;
                image = c.Asset;
                timeout = c.Timeout;
            }
            else
            {
                image = default(Guid);
                timeout = 0;
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
