using System;
using System.IO;

namespace RobotRuntime.Assets
{
    public class PluginImporter : AssetImporter
    {
        public PluginImporter(string path) : base(path) { }

        protected override object LoadAsset()
        {
            return File.ReadAllText(Path);
        }

        public override void SaveAsset()
        {
            File.WriteAllText(Path, Value as string);
        }

        public override Type HoldsType()
        {
            return typeof(string);
        }
    }
}
