using RobotRuntime.Recordings;
using System;

namespace Robot.Abstractions
{
    public interface IHierarchyManager : IBaseHierarchyManager
    {
        Recording ActiveRecording { get; set; }

        event Action<Recording, Recording> ActiveRecordingChanged;
        event Action<Recording> RecordingSaved;

        Recording LoadRecording(string path);
        void SaveRecording(Recording recording, string path);
    }
}
