using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Tests;
using System.Drawing;

namespace RobotRuntime.Execution
{
    public class ForTextCommandRunner : NestedCommandRunner, IRunner
    {
        protected readonly IRuntimeAssetManager RuntimeAssetManager;
        protected readonly IDetectionManager DetectionManager;
        protected readonly ILogger Logger;
        public ForTextCommandRunner(ITextDetectionManager DetectionManager, IRuntimeAssetManager RuntimeAssetManager, ILogger Logger)
        {
            this.RuntimeAssetManager = RuntimeAssetManager;
            this.Logger = Logger;
            this.DetectionManager = DetectionManager;
        }

        private Point[] m_Points;

        /// <summary>
        /// Gets image from attached assets. Finds that image on screen and saves coordinates
        /// </summary>
        protected override TestStatus BeforeRunningParentCommand(ref Command command)
        {
            var textCommand = command as CommandForText;

            if (Logger.AssertIf(textCommand == null, "Command is not ForTextCommand, test runner is not supported: " + command.ToString()))
                return TestStatus.Failed;

            m_Points = DetectionManager.FindImage(Detectable.FromText(textCommand.Text), "Tesseract", textCommand.Timeout).Result;
            if (m_Points == null || m_Points.Length == 0)
                return TestStatus.Failed;

            return TestData.TestStatus;
        }

        protected override TestStatus RunChildCommands(Command[] commands)
        {
            foreach (var p in m_Points)
            {
                if (TestData.IsTestFinished)
                    return TestData.TestStatus;

                foreach (var c in commands)
                {
                    c.SetPropertyIfExist("X", p.X);
                    c.SetPropertyIfExist("Y", p.Y);
                }

                base.RunChildCommands(commands);
                if (TestData.IsTestFinished)
                    return TestData.TestStatus;
            }

            return TestData.TestStatus;
        }
    }
}
