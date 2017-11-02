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

            else
                return null;
        }
    }
}
