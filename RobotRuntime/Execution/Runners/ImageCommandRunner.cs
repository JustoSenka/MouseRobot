using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Settings;
using RobotRuntime.Tests;
using System;
using System.Drawing;

namespace RobotRuntime.Execution
{
    public class ImageCommandRunner : NestedCommandRunner, IRunner
    {
        protected readonly IRuntimeAssetManager RuntimeAssetManager;
        protected readonly IDetectionManager DetectionManager;
        protected readonly ILogger Logger;
        public ImageCommandRunner(IFeatureDetectionManager DetectionManager, IRuntimeAssetManager RuntimeAssetManager, ILogger Logger)
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
            GetImageAndTimeout(command, out Guid imageGuid, out int timeout, out string DetectionMode);

            if (Logger.AssertIf(imageGuid.IsDefault(), "Command does not have valid image referenced: " + command.ToString()))
                return TestStatus.Failed;

            var image = RuntimeAssetManager.GetAsset<Bitmap>(imageGuid);
            if (image == null)
                return TestStatus.Failed;

            m_Points = DetectionManager.FindImage(Detectable.FromBitmap(image), DetectionMode, timeout).Result;
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

        private static void GetImageAndTimeout(Command command, out Guid image, out int timeout, out string DetectionMode)
        {
            if (command is CommandForImage c)
            {
                image = c.Asset;
                timeout = c.Timeout;
                DetectionMode = c.DetectionMode;
            }
            else
            {
                image = default;
                timeout = 0;
                DetectionMode = DetectorNamesHardcoded.Default;
            }
        }
    }
}
