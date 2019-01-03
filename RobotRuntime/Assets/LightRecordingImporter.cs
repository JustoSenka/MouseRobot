using RobotRuntime.IO;
using System;

namespace RobotRuntime.Assets
{
    public class LightRecordingImporter : AssetImporter
    {
        public LightRecordingImporter(string path) : base(path) { }

        protected override object LoadAsset()
        {
            return new YamlRecordingIO().LoadObject<LightRecording>(Path);
        }

        public override void SaveAsset()
        {
            new YamlRecordingIO().SaveObject(Path, (LightRecording)Value);
        }

        public override Type HoldsType()
        {
            return typeof(LightRecording);
        }
    }
}
