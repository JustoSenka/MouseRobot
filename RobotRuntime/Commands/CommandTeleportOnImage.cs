using RobotRuntime.Utils.Win32;
using System;
using System.Drawing;

namespace RobotRuntime.Commands
{
    [Serializable]
    public class CommandTeleportOnImage : Command
    {
        public Bitmap Image { get; set; }

        public CommandTeleportOnImage(Bitmap image)
        {
            Image = image;
        }

        public override object Clone()
        {
            return new CommandTeleportOnImage(Image);
        }

        public override void Run()
        {
            // WinAPI.MouseMoveTo(X, Y);
        }
    }
}
