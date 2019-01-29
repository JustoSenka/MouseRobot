using Robot.Abstractions;
using RobotRuntime.Utils;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace RobotEditor.PropertyUtils
{
    /// <summary>
    /// This will paint a small rect image near the Asset name in property View
    /// </summary>
    public class AssetGUIDImageUITypeEditor : UITypeEditor
    {
        private static IAssetManager AssetManager { get; set; }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            var path = e.Value.ToString();
            if (path.IsEmpty())
                return;

            var asset = AssetManager.GetAsset(e.Value.ToString());
            if (asset == null)
                return;
            
            var bmp = asset.Importer.Load<Bitmap>();
            if (bmp == null)
                return;

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
