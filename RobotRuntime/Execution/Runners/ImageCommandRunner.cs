using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
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

        private Point[] points;

        /// <summary>
        /// Gets image from attached assets. Finds that image on screen and saves coordinates
        /// </summary>
        protected override bool BeforeRunningParentCommand(ref Command command)
        {
            GetImageAndTimeout(command, out Guid imageGuid, out int timeout);

            if (Logger.AssertIf(imageGuid.IsDefault(), "Command does not have valid image referenced: " + command.ToString()))
                return true;

            var image = RuntimeAssetManager.GetAsset<Bitmap>(imageGuid);
            if (image == null)
                return true;

            points = FeatureDetectionThread.FindImageSync(image, timeout);
            if (points == null || points.Length == 0)
                return true;

            return false;
        }

        protected override bool RunChildCommands(Command[] commands)
        {
            foreach (var p in points)
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

        private static void GetImageAndTimeout(Command command, out Guid image, out int timeout)
        {
            if (command is CommandForImage c)
            {
                image = c.Asset;
                timeout = c.Timeout;
            }
            else
            {
                image = default;
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
