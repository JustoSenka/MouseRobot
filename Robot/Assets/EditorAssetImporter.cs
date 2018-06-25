using RobotRuntime;

namespace Robot
{
    public abstract partial class EditorAssetImporter : AssetImporter
    {
        public EditorAssetImporter(string path) : base(path) { }

        public static new AssetImporter FromPath(string path)
        {
            if (path.EndsWith(FileExtensions.Image))
                return new ImageImporter(path);

            else if (path.EndsWith(FileExtensions.Script))
                return new ScriptImporter(path);

            else if (path.EndsWith(FileExtensions.Plugin))
                return new PluginImporter(path);

            else if (path.EndsWith(FileExtensions.Test))
                return new LightTestFixtureImporter(path);

            else
                return null;
        }
    }
}
