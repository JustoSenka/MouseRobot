using System;
using System.Collections.Generic;
using RobotRuntime;
using RobotRuntime.Recordings;

namespace Robot.Abstractions
{
    public interface IHierarchyManager
    {
        Recording ActiveRecording { get; set; }
        IList<Recording> LoadedRecordings { get; }

        event Action<Recording, Recording> ActiveRecordingChanged;
        event Action<Recording, Command, Command> CommandAddedToRecording;
        event Action<Recording, Command, Command, int> CommandInsertedInRecording;
        event Action<Recording, Command, Command> CommandModifiedOnRecording;
        event Action<Recording, Command, int> CommandRemovedFromRecording;
        event Action<Recording> RecordingAdded;
        event Action<Recording> RecordingModified;
        event Action RecordingPositioningChanged;
        event Action<int> RecordingRemoved;
        event Action<Recording> RecordingSaved;

        IEnumerator<Recording> GetEnumerator();
        Recording GetRecordingFromCommand(Command command);
        Recording GetRecordingFromCommandGuid(Guid guid);
        Recording LoadRecording(string path);
        void MoveCommandAfter(Command source, Command after, int recordingIndex, int destinationRecordingIndex = -1);
        void MoveCommandBefore(Command source, Command before, int recordingIndex, int destinationRecordingIndex = -1);
        void MoveRecordingAfter(int index, int after);
        void MoveRecordingBefore(int index, int before);
        Recording NewRecording(Recording clone = null);
        void RemoveRecording(Recording recording);
        void RemoveRecording(int position);
        void SaveRecording(Recording recording, string path);
    }
}