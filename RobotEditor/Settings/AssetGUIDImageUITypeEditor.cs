using Robot;
using Robot.Abstractions;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Unity;

namespace RobotEditor.Settings
{
    class AssetGUIDImageUITypeEditor : UITypeEditor
    {
        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            var AssetManager = RobotRuntime.Unity.Container.Resolve<IAssetManager>();
            var asset = AssetManager.GetAsset(AssetManager.ImageFolder, e.Value.ToString());
            if (asset == null)
                return;
            
            var bmp = asset.Importer.Load<Bitmap>();

            RemoveImageRectangleBounds(e);
            Rectangle destRect = e.Bounds;

            e.Graphics.DrawImage(bmp, destRect);
        }

        private static void RemoveImageRectangleBounds(PaintValueEventArgs e)
        {
            e.Graphics.ExcludeClip(new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, 1));
            e.Graphics.ExcludeClip(new Rectangle(e.Bounds.X, e.Bounds.Y, 1, e.Bounds.Height));
            e.Graphics.ExcludeClip(new Rectangle(e.Bounds.Width, e.Bounds.Y, 1, e.Bounds.Height));
            e.Graphics.ExcludeClip(new Rectangle(e.Bounds.X, e.Bounds.Height, e.Bounds.Width, 1));
        }
    }
}
