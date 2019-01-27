using System;

namespace Robot.Assets
{
    /// <summary>
    /// Useless importer, it is only here so directory can appear as an asset in AssetManager and AssetsWindow
    /// </summary>
    public class DirectoryImporter : EditorAssetImporter
    {
        public DirectoryImporter(string path) : base(path) { }

        protected override object LoadAsset() => Path;

        public override void SaveAsset() { }

        public override Type HoldsType() => typeof(string);
    }
}
