using System;
using System.Drawing;
using Graphics = System.Drawing.Graphics;

namespace Robot
{
    public interface AssetImporter<T>
    {
        T Asset { get; }
    }

    public class ImageImporter : AssetImporter<Bitmap>
    {
        public Bitmap Asset { get; private set; }

        public ImageImporter(string path)
        {
            Asset = new Bitmap(path);
        }
    }

    public class ScriptImporter : AssetImporter<Script>
    {
        public Script Asset { get; private set; }

        public ScriptImporter(string path)
        {
            Asset = BinaryObjectIO.LoadObject<Script>(path);
        }
    }
}
