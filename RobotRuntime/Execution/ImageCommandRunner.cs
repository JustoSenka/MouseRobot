using RobotRuntime.Assets;
using RobotRuntime.Commands;
using RobotRuntime.Graphics;
using RobotRuntime.Utils;
using RobotRuntime.Utils.Win32;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace RobotRuntime.Execution
{
    [SupportedType(typeof(CommandForeachImage))]
    [SupportedType(typeof(CommandForImage))]
    public class ImageCommandRunner : IRunner
    {

        // TODO: Whent test fixture or test class is introduced, replace this with Test. It needs to know test hierarchy, that's why it is like that
        // TODO: Or maybe it's fine to know just the script he command is on?
        private LightScript m_TestFixture;
        private CommandRunningCallback m_Callback;

        public ImageCommandRunner(LightScript testFixture, CommandRunningCallback callback)
        {
            m_TestFixture = testFixture;
            m_Callback = callback;
        }

        public void Run(IRunnable runnable)
        {
            if (!RunnerFactory.DoesRunnerSupportType(this.GetType(), runnable.GetType()))
                throw new ArgumentException("This runner '" + this + "' is not compatible with this type: '" + runnable.GetType());

            var command = runnable as Command;
            var commandNode = m_TestFixture.Commands.FirstOrDefault(node => node.value == command);
            RunCommand(commandNode);
        }

        private void RunCommand(TreeNode<Command> node)
        {
            Guid imageGuid;
            int timeout;
            GetImageAndTimeout(node, out imageGuid, out timeout);

            var path = AssetGuidManager.Instance.GetPath(imageGuid);
            var points = GetCoordinates(node, path, timeout);
            if (points == null || points.Length == 0)
                return;

            foreach (var p in points)
            {
                m_Callback.Invoke(node.value);
                node.value.Run();

                foreach (var childNode in node)
                {
                    OverrideCommandPropertiesIfExist(childNode.value, p.X, "X");
                    OverrideCommandPropertiesIfExist(childNode.value, p.Y, "Y");

                    var runner = RunnerFactory.CreateFor(childNode.value.GetType());
                    runner.Run(childNode.value);
                }
            }
        }

        private static Point[] GetCoordinates(TreeNode<Command> node, string imagePath, int timeout)
        {
            var command = node.value;

            if (node.value.CommandType != CommandType.ForeachImage && node.value.CommandType != CommandType.ForImage)
                return null;

            int x1 = WinAPI.GetCursorPosition().X;
            int y1 = WinAPI.GetCursorPosition().Y;

            FeatureDetectionThread.Instace.StartNewImageSearch(imagePath);
            while (timeout > FeatureDetectionThread.Instace.TimeSinceLastFind)
            {
                Task.Delay(5).Wait(); // It will probably wait 15-30 ms, depending on thread clock, find better solution
                if (FeatureDetectionThread.Instace.WasImageFound)
                    break;
            }

            if (FeatureDetectionThread.Instace.WasImageFound)
                return FeatureDetectionThread.Instace.LastKnownPositions.Select(p => p.FindCenter()).ToArray();
            else
                return null;
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
