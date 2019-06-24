using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Tests;
using System.Drawing;

namespace RobotRuntime.Execution
{
    public class CommandIfImageVisibleRunner : NestedCommandRunner, IRunner
    {
        protected readonly IRuntimeAssetManager RuntimeAssetManager;
        protected readonly IDetectionManager DetectionManager;
        protected readonly ILogger Logger;
        public CommandIfImageVisibleRunner(IFeatureDetectionManager DetectionManager, IRuntimeAssetManager RuntimeAssetManager, ILogger Logger)
        {
            this.RuntimeAssetManager = RuntimeAssetManager;
            this.Logger = Logger;
            this.DetectionManager = DetectionManager;
        }

        private Point[] m_Points;
        private bool WasImageFound = false;
        private bool ShouldContinueCommandExecution = false;

        /// <summary>
        /// Gets image from attached assets. Finds that image on screen and saves coordinates
        /// </summary>
        protected override TestStatus BeforeRunningParentCommand(ref Command baseCommand)
        {
            if (!(baseCommand is CommandIfImageVisible command))
            {
                Logger.Logi(LogType.Error, "This runner '" + this + "' is not compatible with this type: '" + baseCommand.GetType());
                return TestStatus.Failed;
            }

            if (Logger.AssertIf(command.Asset.IsDefault(), "Command does not have valid image referenced: " + command.ToString()))
                return TestStatus.Failed;

            var image = RuntimeAssetManager.GetAsset<Bitmap>(command.Asset);
            if (image == null)
                return TestStatus.Failed;

            m_Points = DetectionManager.FindImage(Detectable.FromBitmap(image), command.DetectionMode, command.Timeout).Result;
            WasImageFound = !(m_Points == null || m_Points.Length == 0);

            // If image was not found, but we expect it to be found. Return false and not fail the test on purpose, 
            // Make sure to not run commands nested inside
            // Same with if image was found, but we wanted it to be not found
            ShouldContinueCommandExecution = (command.ExpectTrue && WasImageFound) || (!command.ExpectTrue && !WasImageFound);

            return TestStatus.None;
        }

        protected override TestStatus RunChildCommands(Command[] commands)
        {
            if (!ShouldContinueCommandExecution)
                return TestStatus.None;

            if (TestData.IsTestFinished)
                return TestData.TestStatus;

            // TODO: Should this also have foreach version of it?
            if (WasImageFound)
            {
                foreach (var c in commands)
                {
                    c.SetPropertyIfExist("X", m_Points[0].X);
                    c.SetPropertyIfExist("Y", m_Points[0].Y);
                }
            }

            return base.RunChildCommands(commands);
        }
    }
}
