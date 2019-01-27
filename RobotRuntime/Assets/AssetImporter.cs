using RobotRuntime.Assets;
using System;

namespace RobotRuntime
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
                    catch (Exception e)
                    {
                        LoadingFailed = true;
                        Logger.Log(LogType.Error, "Failed to load asset: " + Path, e.Message);
                    }

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

        public string Path;
        protected abstract object LoadAsset();
        public abstract void SaveAsset();
        public abstract Type HoldsType();
        public bool LoadingFailed { get; private set; }

        public static AssetImporter FromPath(string path)
        {
            if (path.EndsWith(FileExtensions.Image))
                return new ImageImporter(path);

            else if (path.EndsWith(FileExtensions.Recording))
                return new LightRecordingImporter(path);

            else if (path.EndsWith(FileExtensions.Script))
                return new ScriptImporter(path);

            else if (path.EndsWith(FileExtensions.Test))
                return new LightTestFixtureImporter(path);

            else if (path.EndsWith(FileExtensions.Dll) || path.EndsWith(FileExtensions.Exe))
                return new PluginImporter(path);

            else
                return new TextAssetImporter(path);
        }
    }
}
