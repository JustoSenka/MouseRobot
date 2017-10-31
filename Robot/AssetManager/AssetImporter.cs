using RobotRuntime;
using RobotRuntime.IO;
using System;
using System.Drawing;
using System.Drawing.Imaging;

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
            set
            {
                m_AssetValue = value;
            }
            get
            {
                if (m_AssetValue == null && !LoadingFailed)
                    try { m_AssetValue = LoadAsset(); }
                    catch (Exception) { LoadingFailed = true; }

                return m_AssetValue;
            }
        }

        public T Load<T>()
        {
            return (T)Value;
        }

        public T ReloadAsset<T>()
        {
            LoadingFailed = false;

            try { m_AssetValue = LoadAsset(); }
            catch (Exception) { LoadingFailed = true; }

            return (T)Value;
        }

        internal string Path;
        protected abstract object LoadAsset();
        public abstract void SaveAsset();
        public abstract Type HoldsType();
        public bool LoadingFailed { get; private set; }

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

            public override void SaveAsset()
            {
                ((Bitmap)Value).Save(Path, ImageFormat.Png);
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
                return new Script(ObjectIO.Create().LoadObject<LightScript>(Path));
            }

            public override void SaveAsset()
            {
                ObjectIO.Create().SaveObject(Path, ((Script)Value).ToLightScript());
            }

            public override Type HoldsType()
            {
                return typeof(Script);
            }
        }
    }
}
