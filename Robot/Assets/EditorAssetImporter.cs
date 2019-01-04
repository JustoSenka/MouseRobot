using Robot.Assets;
using RobotRuntime;
using RobotRuntime.Assets;

namespace Robot
{
    public abstract class EditorAssetImporter : AssetImporter
    {
        public EditorAssetImporter(string path) : base(path) { }

        public static new AssetImporter FromPath(string path)
        {
            if (path.EndsWith(FileExtensions.ImageD))
                return new ImageImporter(path);

            else if (path.EndsWith(FileExtensions.RecordingD))
                return new RecordingImporter(path);

            else if (path.EndsWith(FileExtensions.ScriptD))
                return new ScriptImporter(path);

            else if (path.EndsWith(FileExtensions.TestD))
                return new LightTestFixtureImporter(path);

            else if (path.EndsWith(FileExtensions.DllD) || path.EndsWith(FileExtensions.ExeD))
                return new PluginImporter(path);

            else
                return null;
        }
    }
}
