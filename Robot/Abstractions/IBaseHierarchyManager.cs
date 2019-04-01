using System;
using System.Collections.Generic;
using RobotRuntime;
using RobotRuntime.Recordings;

namespace Robot.Abstractions
{
    public interface IBaseHierarchyManager : IEnumerable<Recording>, IHaveGuidMap
    {
        IList<Recording> LoadedRecordings { get; }

        event Action<Recording, Command, Command> CommandAddedToRecording;
        event Action<Recording, Command, Command, int> CommandInsertedInRecording;
        event Action<Recording, Command, Command> CommandModifiedOnRecording;
        event Action<Recording, Command, int> CommandRemovedFromRecording;

        event Action<Recording> RecordingAdded;
        event Action<Recording> RecordingModified;
        event Action RecordingPositioningChanged;
        event Action<int> RecordingRemoved;

        Recording AddRecording(Recording recording, bool removeRecordingWithSamePath = false);
        Recording NewRecording(Recording clone = null);
        Recording GetRecordingFromCommand(Command command);
        Recording GetRecordingFromCommandGuid(Guid guid);

        int GetCommandIndex(Command command);
        int GetRecordingIndex(Recording recording);

        void MoveCommandAfter(Command source, Command after, int recordingIndex, int destinationRecordingIndex = -1);
        void MoveCommandBefore(Command source, Command before, int recordingIndex, int destinationRecordingIndex = -1);
        void MoveRecordingAfter(int index, int after);
        void MoveRecordingBefore(int index, int before);

        void RemoveAllRecordings();
        void RemoveRecording(int position);
        void RemoveRecording(Recording recording);
    }
}
