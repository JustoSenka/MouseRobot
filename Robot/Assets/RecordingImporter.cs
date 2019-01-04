using RobotRuntime;
using RobotRuntime.Assets;
using RobotRuntime.IO;
using RobotRuntime.Recordings;
using System;

namespace Robot.Assets
{
    public class RecordingImporter : LightRecordingImporter
    {
        public RecordingImporter(string path) : base(path) { }

        protected override object LoadAsset()
        {
            return new Recording((LightRecording)base.LoadAsset());
        }

        public override void SaveAsset()
        {
            new YamlRecordingIO().SaveObject(Path, ((Recording)Value).ToLightRecording());
        }

        public override Type HoldsType()
        {
            return typeof(Recording);
        }
    }
}
