using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Recordings;

namespace Robot
{
    public static class CommandExtension
    {
        public static int GetIndex(this Command command, IBaseHierarchyManager RecordingManager)
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

        public static int GetIndex(this Recording recording, IBaseHierarchyManager RecordingManager)
        {
            return RecordingManager.LoadedRecordings.IndexOf(recording);
        }
    }
}
