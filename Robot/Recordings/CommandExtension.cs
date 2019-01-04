using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Recordings;

namespace Robot
{
    public static class CommandExtension
    {
        public static int GetIndex(this Command command, IHierarchyManager RecordingManager)
        {
            var recording = RecordingManager.GetRecordingFromCommand(command);
            var node = recording.Commands.GetNodeFromValue(command);
            return node.parent.IndexOf(command);
        }

        public static int GetIndex(this Command command, Recording recording)
        {
            var node = recording.Commands.GetNodeFromValue(command);
            return node.parent.IndexOf(command);
        }

        public static int GetIndex(this Recording recording, IHierarchyManager RecordingManager)
        {
            return RecordingManager.LoadedRecordings.IndexOf(recording);
        }
    }
}
