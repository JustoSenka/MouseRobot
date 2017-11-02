using RobotRuntime.Utils.Win32;
using System;
using System.Drawing;

namespace RobotRuntime.Commands
{
    [Serializable]
    public class CommandTeleportOnImage : Command
    {
        public AssetPointer Asset { get; set; }

        public CommandTeleportOnImage(AssetPointer asset)
        {
            Asset = asset;
        }

        public override object Clone()
        {
            return new CommandTeleportOnImage(Asset);
        }

        public override void Run()
        {
            // WinAPI.MouseMoveTo(X, Y);
        }
    }
}
