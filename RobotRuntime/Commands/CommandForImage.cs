using RobotRuntime.Graphics;
using RobotRuntime.Utils.Win32;
using System;
using System.Threading.Tasks;

namespace RobotRuntime.Commands
{
    [Serializable]
    public class CommandForImage : Command
    {
        public AssetPointer Asset { get; set; }
        public int Timeout { get; set; }

        public CommandForImage(AssetPointer asset, int timeOut)
        {
            Asset = asset;
            Timeout = timeOut;
        }

        public override object Clone()
        {
            return new CommandForImage(Asset, Timeout);
        }

        public override void Run()
        {
            int x1, y1;
            x1 = WinAPI.GetCursorPosition().X;
            y1 = WinAPI.GetCursorPosition().Y;

            FeatureDetectionThread.Instace.StartNewImageSearch(Asset);
            while (Timeout > FeatureDetectionThread.Instace.TimeSinceLastFind)
            {
                Task.Delay(5).Wait(); // It will probably wait 15-30 ms, depending on thread clock, find better solution
                if (FeatureDetectionThread.Instace.WasImageFound)
                {
                    var p = FeatureDetectionThread.Instace.LastKnownPositions[0].FindCenter();


                    break;
                }
            }
        }

        public override string ToString()
        {
            var assetName = ((Asset.Path != "" && Asset.Path != null) ? Commons.GetName(Asset.Path) : "...");
            return "For image '" + assetName + "':";
        }

        public override CommandType CommandType { get { return CommandType.ForImage; } }
    }
}
