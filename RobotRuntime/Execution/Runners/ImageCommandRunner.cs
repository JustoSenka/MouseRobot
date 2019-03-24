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
        protected readonly IFeatureDetectionThread FeatureDetectionThread;
        protected readonly ILogger Logger;
        public ImageCommandRunner(IFeatureDetectionThread FeatureDetectionThread, IRuntimeAssetManager RuntimeAssetManager, ILogger Logger)
        {
            this.RuntimeAssetManager = RuntimeAssetManager;
            this.Logger = Logger;
            this.FeatureDetectionThread = FeatureDetectionThread;
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

            m_Points = FeatureDetectionThread.FindImageSync(image, DetectionMode, timeout);
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
