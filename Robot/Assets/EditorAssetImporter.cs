using Robot.Assets;
using RobotRuntime;
using RobotRuntime.Assets;
using RobotRuntime.Utils;

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

            else if (Paths.IsDirectory(path))
                return new DirectoryImporter(path);

            else
                return new TextAssetImporter(path);
        }
    }
}
