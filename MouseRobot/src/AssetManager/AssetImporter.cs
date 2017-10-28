using Robot.IO;
using System;
using System.Drawing;
using Graphics = System.Drawing.Graphics;

namespace Robot
{
    public abstract class AssetImporter
    {
        public AssetImporter(string path)
        {
            Path = path;
        }

        private object m_AssetValue;
        public object Value
        {
            get
            {
                if (m_AssetValue == null)
                    m_AssetValue = LoadAsset();

                return m_AssetValue;
            }
        }

        public T Cast<T>()
        {
            return (T) Value;
        }

        protected string Path;
        protected abstract object LoadAsset();
        public abstract Type HoldsType();

        public static AssetImporter FromPath(string path)
        {
            if (path.EndsWith(FileExtensions.Image))
                return new ImageImporter(path);

            else if (path.EndsWith(FileExtensions.Script))
                return new ScriptImporter(path);

            else
                return null;
        }


        private class ImageImporter : AssetImporter
        {
            public ImageImporter(string path) : base(path) { }

            protected override object LoadAsset()
            {
                return new Bitmap(Path);
            }

            public override Type HoldsType()
            {
                return typeof(Bitmap);
            }
        }


        private class ScriptImporter : AssetImporter
        {
            public ScriptImporter(string path) : base(path) { }

            protected override object LoadAsset()
            {
                return ObjectIO.Create().LoadObject<Script>(Path);
            }

            public override Type HoldsType()
            {
                return typeof(Script);
            }
        }
    }
}
