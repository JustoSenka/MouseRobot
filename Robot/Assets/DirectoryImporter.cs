using System;
using System.IO;

namespace Robot.Assets
{
    /// <summary>
    /// Useless importer, it is only here so directory can appear as an asset in AssetManager and AssetsWindow
    /// </summary>
    public class DirectoryImporter : EditorAssetImporter
    {
        public DirectoryImporter(string path) : base(path) { }

        protected override object LoadAsset() => Path;

        public override void SaveAsset() => Directory.CreateDirectory(Path); 

        public override Type HoldsType() => typeof(string);
    }
}
