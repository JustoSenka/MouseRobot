using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace RobotRuntime
{
    public abstract partial class AssetImporter
    {
        protected class ImageImporter : AssetImporter
        {
            public ImageImporter(string path) : base(path) { }

            protected override object LoadAsset()
            {
                return new Bitmap(Path);
            }

            public override void SaveAsset()
            {
                ((Bitmap)Value).Save(Path, ImageFormat.Png);
            }

            public override Type HoldsType()
            {
                return typeof(Bitmap);
            }
        }
    }
}
