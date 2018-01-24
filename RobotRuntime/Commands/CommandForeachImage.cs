using RobotRuntime.Assets;
using RobotRuntime.Graphics;
using RobotRuntime.Utils.Win32;
using System;
using System.Threading.Tasks;

namespace RobotRuntime.Commands
{
    [Serializable]
    public class CommandForeachImage : Command
    {
        public Guid Asset { get; set; }
        public int Timeout { get; set; }

        public CommandForeachImage(Guid asset, int timeOut)
        {
            Asset = asset;
            Timeout = timeOut;
        }

        public override object Clone()
        {
            return new CommandForeachImage(Asset, Timeout);
        }

        public override void Run()
        {
            /*int x1, y1;
            x1 = WinAPI.GetCursorPosition().X;
            y1 = WinAPI.GetCursorPosition().Y;

            var path = AssetGuidManager.Instance.GetPath(Asset);
            FeatureDetectionThread.Instace.StartNewImageSearch(path);
            while (Timeout > FeatureDetectionThread.Instace.TimeSinceLastFind)
            {
                Task.Delay(5).Wait(); // It will probably wait 15-30 ms, depending on thread clock, find better solution
                if (FeatureDetectionThread.Instace.WasImageFound)
                {
                    var p = FeatureDetectionThread.Instace.LastKnownPositions[0].FindCenter();


                    break;
                }
            }*/
        }

        public override string ToString()
        {
            var path = AssetGuidManager.Instance.GetPath(Asset);
            var assetName = ((path != "" && path != null) ? Commons.GetName(path) : "...");
            return "For Each image '" + assetName + "':";
        }

        public override CommandType CommandType { get { return CommandType.ForeachImage; } }
    }
}
