using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Robot.Recordings
{
    public abstract class BaseHierarchyManager : IBaseHierarchyManager, IEnumerable<Recording>, IHaveGuidMap
    {
        protected readonly IList<Recording> m_LoadedRecordings;
        public IList<Recording> LoadedRecordings { get { return m_LoadedRecordings; } }

        public event Action<Recording> RecordingAdded;
        public event Action<Recording> RecordingModified;
        public event Action<int> RecordingRemoved;
        public event Action RecordingPositioningChanged;

        public event Action<Recording, Command, Command> CommandAddedToRecording;
        public event Action<Recording, Command, Command, int> CommandInsertedInRecording;
        public event Action<Recording, Command, int> CommandRemovedFromRecording;
        public event Action<Recording, Command, Command> CommandModifiedOnRecording;

        protected readonly HashSet<Guid> RecordingGuidMap = new HashSet<Guid>();

        private ICommandFactory CommandFactory;
        private IProfiler Profiler;
        private ILogger Logger;
        public BaseHierarchyManager(ICommandFactory CommandFactory, IProfiler Profiler, ILogger Logger)
        {
            this.CommandFactory = CommandFactory;
            this.Profiler = Profiler;
            this.Logger = Logger;

            CommandFactory.NewUserCommands += ReplaceCommandsInRecordingsWithNewRecompiledOnes;

            m_LoadedRecordings = new List<Recording>();
        }

        protected void SubscribeToRecordingEvents(Recording s)
        {
            s.CommandAddedToRecording += InvokeCommandAddedToRecording;
            s.CommandInsertedInRecording += InvokeCommandInsertedInRecording;
            s.CommandRemovedFromRecording += InvokeCommandRemovedFromRecording;
            s.CommandModifiedOnRecording += InvokeCommandModifiedOnRecording;
        }

        protected void UnsubscribeToRecordingEvents(Recording s)
        {
            s.CommandAddedToRecording -= InvokeCommandAddedToRecording;
            s.CommandInsertedInRecording -= InvokeCommandInsertedInRecording;
            s.CommandRemovedFromRecording -= InvokeCommandRemovedFromRecording;
            s.CommandModifiedOnRecording -= InvokeCommandModifiedOnRecording;
        }

        private void InvokeCommandAddedToRecording(Recording recording, Command parentCommand, Command command)
        {
            CommandAddedToRecording?.Invoke(recording, parentCommand, command);
        }

        private void InvokeCommandInsertedInRecording(Recording recording, Command parentCommand, Command command, int index)
        {
            CommandInsertedInRecording?.Invoke(recording, parentCommand, command, index);
        }

        private void InvokeCommandRemovedFromRecording(Recording recording, Command parentCommand, int index)
        {
            CommandRemovedFromRecording?.Invoke(recording, parentCommand, index);
        }

        private void InvokeCommandModifiedOnRecording(Recording recording, Command oldCommand, Command newCommand)
        {
            CommandModifiedOnRecording?.Invoke(recording, oldCommand, newCommand);
        }

        private void ReplaceCommandsInRecordingsWithNewRecompiledOnes()
        {
            Profiler.Start("BaseHierarchyManager_ReplaceOldCommandInstances");

            foreach (var recording in LoadedRecordings)
            {
                foreach (var node in recording.Commands.GetAllNodes().ToArray())
                {
                    var command = node.value;
                    if (command == null || CommandFactory.IsNative(command))
                        continue;

                    recording.ReplaceCommand(node.value, CommandFactory.Create(node.value.Name, node.value));
                }
            }

            Profiler.Stop("BaseHierarchyManager_ReplaceOldCommandInstances");
        }

        public virtual Recording NewRecording(Recording clone = null)
        {
            Recording recording;

            if (clone == null)
                recording = new Recording();
            else
            {
                recording = (Recording)clone.Clone();
                ((IHaveGuid)recording).RegenerateGuid();
            }

            RecordingGuidMap.AddGuidToMapAndGenerateUniqueIfNeeded(recording);
            m_LoadedRecordings.Add(recording);

            SubscribeToRecordingEvents(recording);
            recording.IsDirty = true;

            RecordingAdded?.Invoke(recording);
            return recording;
        }

        public virtual void RemoveRecording(Recording recording)
        {
            var position = m_LoadedRecordings.IndexOf(recording);

            RecordingGuidMap.RemoveGuidFromMap(recording);
            m_LoadedRecordings.Remove(recording);
            UnsubscribeToRecordingEvents(recording);

            RecordingRemoved?.Invoke(position);
        }

        public virtual void RemoveRecording(int position)
        {
            UnsubscribeToRecordingEvents(m_LoadedRecordings[position]);

            RecordingGuidMap.RemoveGuidFromMap(m_LoadedRecordings[position]);
            m_LoadedRecordings.RemoveAt(position);

            RecordingRemoved?.Invoke(position);
        }

        /// <summary>
        /// This method should be used to unload everything and call proper callbacks.
        /// LoadedRecordings.Clear should not be used since it will not update UI
        /// </summary>
        public virtual void RemoveAllRecordings()
        {
            for (int i = m_LoadedRecordings.Count - 1; i >= 0; --i)
                RemoveRecording(i);
        }

        public virtual Recording AddRecording(Recording recording, bool removeRecordingWithSamePath = false)
        {
            if (recording == null)
                return null;

            // If recording was already loaded, reload it to last saved state
            var oldRecording = m_LoadedRecordings.FirstOrDefault(s => s.Path.Equals(recording.Path));
            if (oldRecording != default(Recording) && removeRecordingWithSamePath)
            {
                // Reload Recording
                var index = m_LoadedRecordings.IndexOf(oldRecording);
                UnsubscribeToRecordingEvents(oldRecording);

                RecordingGuidMap.RemoveGuidFromMap(m_LoadedRecordings[index]);
                RecordingGuidMap.AddGuidToMapAndGenerateUniqueIfNeeded(recording);

                m_LoadedRecordings[index] = recording;
                RecordingModified?.Invoke(recording);
            }
            else
            {
                // Load New Recording
                RecordingGuidMap.AddGuidToMapAndGenerateUniqueIfNeeded(recording);
                m_LoadedRecordings.Add(recording);
                RecordingAdded?.Invoke(recording);
            }

            SubscribeToRecordingEvents(recording);
            return recording;
        }

        /// <summary>
        /// Moves existing command from recording to different place and/or between recordings.
        /// Also moves all child commands altogether.
        /// </summary>
        public void MoveCommandAfter(Command source, Command after, int recordingIndex, int destinationRecordingIndex = -1) // recording indices could be removed
        {
            var recording = m_LoadedRecordings[recordingIndex];

            if (recordingIndex == destinationRecordingIndex || destinationRecordingIndex == -1) // Same recording
                recording.MoveCommandAfter(source, after);

            else // Move between two different recordings
            {
                var destRecording = m_LoadedRecordings[destinationRecordingIndex];
                var sourceNode = recording.Commands.GetNodeFromValue(source);

                if (Logger.AssertIf(sourceNode == null,
                    "Cannot find node in recording '" + destRecording.Name + "' for command: " + source))
                    return;

                recording.RemoveCommand(source);
                destRecording.InsertCommandNodeAfter(sourceNode, after);

                destRecording.IsDirty = true;
            }

            recording.IsDirty = true;
        }

        /// <summary>
        /// Moves existing command from recording to different place and/or between recordings.
        /// Also moves all child commands altogether.
        /// </summary>
        public void MoveCommandBefore(Command source, Command before, int recordingIndex, int destinationRecordingIndex = -1) // recording indices could be removed
        {
            var recording = m_LoadedRecordings[recordingIndex];

            if (recordingIndex == destinationRecordingIndex || destinationRecordingIndex == -1) // Same recording
                recording.MoveCommandBefore(source, before);

            else // Move between two different recordings
            {
                var destRecording = m_LoadedRecordings[destinationRecordingIndex];
                var sourceNode = m_LoadedRecordings[recordingIndex].Commands.GetNodeFromValue(source);

                if (Logger.AssertIf(sourceNode == null,
                    "Cannot find node in recording '" + destRecording.Name + "' for command: " + source))
                    return;

                recording.RemoveCommand(source);
                destRecording.InsertCommandNodeBefore(sourceNode, before);

                destRecording.IsDirty = true;
            }

            recording.IsDirty = true;
        }

        public void MoveRecordingAfter(int index, int after)
        {
            m_LoadedRecordings.MoveAfter(index, after);
            RecordingPositioningChanged?.Invoke();
        }

        public void MoveRecordingBefore(int index, int before)
        {
            m_LoadedRecordings.MoveBefore(index, before);
            RecordingPositioningChanged?.Invoke();
        }

        /// <summary>
        /// Iterates all recordings and checks guid map to find specific command.
        /// </summary>
        public Recording GetRecordingFromCommand(Command command)
        {
            return LoadedRecordings.FirstOrDefault(r => r.HasRegisteredGuid(command.Guid));
            // return LoadedRecordings.FirstOrDefault((s) => s.Commands.GetAllNodes(false).Select(n => n.value).Contains(command));
        }

        /// <summary>
        /// Iterates all recordings and checks guid map to find specific command.
        /// </summary>
        public Recording GetRecordingFromCommandGuid(Guid guid)
        {
            return LoadedRecordings.FirstOrDefault(r => r.HasRegisteredGuid(guid));
            // return LoadedRecordings.FirstOrDefault((s) => s.Commands.GetAllNodes(false).Select(n => n.value.Guid).Contains(guid));
        }

        public int GetCommandIndex(Command command)
        {
            var recording = GetRecordingFromCommand(command);
            var node = recording.Commands.GetNodeFromValue(command);
            return node.parent.IndexOf(command);
        }

        public int GetRecordingIndex(Recording recording)
        {
            return LoadedRecordings.IndexOf(recording);
        }

        public bool HasRegisteredGuid(Guid guid)
        {
            return RecordingGuidMap.Contains(guid);
        }

        public IEnumerator<Recording> GetEnumerator()
        {
            return m_LoadedRecordings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_LoadedRecordings.GetEnumerator();
        }

        [Conditional("DEBUG")]
        private void CheckCommandGuidConsistency()
        {
            foreach (var s in m_LoadedRecordings)
            {
                if (!RecordingGuidMap.Contains(s.Guid))
                    Logger.Logi(LogType.Error, "Recording is not registered to guid map: " + s.ToString());
            }
        }
    }
}
