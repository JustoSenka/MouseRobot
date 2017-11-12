using RobotRuntime.Graphics;
using RobotRuntime.Utils.Win32;
using System;
using System.Threading.Tasks;

namespace RobotRuntime.Commands
{
    [Serializable]
    public class CommandMoveOnImage : Command
    {
        public AssetPointer Asset { get; set; }
        public int Timeout { get; set; }
        public bool Smooth { get; set; }

        public CommandMoveOnImage(AssetPointer asset, int timeOut, bool smooth)
        {
            Asset = asset;
            Timeout = timeOut;
            Smooth = smooth;
        }

        public override object Clone()
        {
            return new CommandMoveOnImage(Asset, Timeout, Smooth);
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
                    // TODO: start searching for image for next command while doing this long operation
                    var p = FeatureDetectionThread.Instace.LastKnownPositions[0].FindCenter();

                    if (Smooth)
                        for (int i = 1; i <= 50; i++)
                            WinAPI.MouseMoveTo(x1 + ((p.X - x1) * i / 50), y1 + ((p.Y - y1) * i / 50));

                    else
                        WinAPI.MouseMoveTo(p.X, p.Y);

                    break;
                }
            }
        }

        public override string ToString()
        {
            if (Smooth)
                return "Smooth Move to image: " + Commons.GetName(Asset.Path);
            else
                return "Move to image: " + Commons.GetName(Asset.Path);
        }
    }
}
