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
            return new RobotRuntime.Recordings.Recording((LightRecording)base.LoadAsset());
        }

        public override void SaveAsset()
        {
            new YamlRecordingIO().SaveObject(Path, ((RobotRuntime.Recordings.Recording)Value).ToLightScript());
        }

        public override Type HoldsType()
        {
            return typeof(RobotRuntime.Recordings.Recording);
        }
    }
}
