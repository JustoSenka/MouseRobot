using BrightIdeasSoftware;
using Robot;
using Robot.Abstractions;
using RobotRuntime;

namespace RobotEditor
{
    public static class BaseHierarchyManagerExtension
    {
        /// <summary>
        /// Will move source recording above or below destination recording according to DropTargetLocation
        /// </summary>
        public static void MoveRecording(this IBaseHierarchyManager hierarchy, DropTargetLocation loc, int source, int destination)
        {
            if (loc == DropTargetLocation.AboveItem)
                hierarchy.MoveRecordingBefore(source, destination);

            else if (loc == DropTargetLocation.BelowItem)
                hierarchy.MoveRecordingAfter(source, destination);

            else
                Logger.Log(LogType.Error, "BaseHierarchyManagerExtension.MoveRecording does not support: " + loc);
        }

        /// <summary>
        /// Will move source command above, below or onto destination command according to DropTargetLocation and if command can be nested
        /// </summary>
        public static void MoveCommand(this IBaseHierarchyManager hierarchy, DropTargetLocation loc, Command source, Command destination)
        {
            var sourceRecording = hierarchy.GetRecordingFromCommand(source);
            var targetRecording = hierarchy.GetRecordingFromCommand(destination);

            if (loc == DropTargetLocation.AboveItem)
                hierarchy.MoveCommandBefore(source, destination, sourceRecording.GetIndex(hierarchy), targetRecording.GetIndex(hierarchy));

            else if (loc == DropTargetLocation.BelowItem)
                hierarchy.MoveCommandAfter(source, destination, sourceRecording.GetIndex(hierarchy), targetRecording.GetIndex(hierarchy));

            else if (loc == DropTargetLocation.Item)
            {
                if (source == destination)
                {
                    Logger.Log(LogType.Warning, "Cannot move command inside itself: " + source.Name);
                    return;
                }
                if (!destination.CanBeNested)
                {
                    Logger.Log(LogType.Warning, "Destination command does not allow nesting: " + destination.Name);
                    return;
                }

                var node = sourceRecording.Commands.GetNodeFromValue(source);
                sourceRecording.RemoveCommand(source);
                targetRecording.AddCommandNode(node, destination);
            }

            else
                Logger.Log(LogType.Error, "BaseHierarchyManagerExtension.MoveCommand does not support: " + loc);
        }

        /// <summary>
        /// Will move source command node above, below or onto destination command according to DropTargetLocation and if command can be nested
        /// </summary>
        public static void InsertCommandNode(this IBaseHierarchyManager hierarchy, DropTargetLocation loc, TreeNode<Command> source, Command destination)
        {
            var targetRecording = hierarchy.GetRecordingFromCommand(destination);

            if (loc == DropTargetLocation.AboveItem)
                targetRecording.InsertCommandNodeBefore(source, destination);

            else if (loc == DropTargetLocation.BelowItem)
                targetRecording.InsertCommandNodeAfter(source, destination);

            else if (loc == DropTargetLocation.Item)
            {
                if (destination.CanBeNested)
                    targetRecording.AddCommandNode(source, destination);
            }

            else
                Logger.Log(LogType.Error, "BaseHierarchyManagerExtension.InsertCommandNode does not support: " + loc);
        }
    }
}
