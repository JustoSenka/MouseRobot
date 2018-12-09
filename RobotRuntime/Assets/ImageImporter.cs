using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace RobotRuntime.Assets
{
    public class ImageImporter : AssetImporter
    {
        public ImageImporter(string path) : base(path) { }

        protected override object LoadAsset()
        {
            Bitmap bmp;
            // This will load bitmap without keeping a lock on the file
            using (var bmpTemp = new Bitmap(Path))
            {
                bmp = new Bitmap(bmpTemp);
            }
            return bmp;
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
