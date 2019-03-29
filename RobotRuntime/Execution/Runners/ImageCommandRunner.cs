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
        public ImageCommandRunner(IDetectionManager DetectionManager, IRuntimeAssetManager RuntimeAssetManager, ILogger Logger)
        {
            this.RuntimeAssetManager = RuntimeAssetManager;
            this.Logger = Logger;
            this.DetectionManager = DetectionManager;
        }

        private Point[] m_Points;

        /// <summary>
        /// Gets image from attached assets. Finds that image on screen and saves coordinates
        /// </summary>
        protected override bool BeforeRunningParentCommand(ref Command command)
        {
            GetImageAndTimeout(command, out Guid imageGuid, out int timeout, out string DetectionMode);

            if (Logger.AssertIf(imageGuid.IsDefault(), "Command does not have valid image referenced: " + command.ToString()))
                return true;

            var image = RuntimeAssetManager.GetAsset<Bitmap>(imageGuid);
            if (image == null)
                return true;

            m_Points = DetectionManager.FindImage(image, DetectionMode, timeout).Result;
            if (m_Points == null || m_Points.Length == 0)
                return true;

            return false;
        }

        protected override bool RunChildCommands(Command[] commands)
        {
            foreach (var p in m_Points)
            {
                if (TestData.ShouldCancelRun || TestData.ShouldFailTest)
                    return true;

                foreach (var c in commands)
                {
                    c.SetPropertyIfExist("X", p.X);
                    c.SetPropertyIfExist("Y", p.Y);
                }

                if (base.RunChildCommands(commands))
                {
                    TestData.ShouldFailTest = true;
                    return true;
                }

                if (TestData.ShouldPassTest)
                    return false;
            }

            return false;
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
