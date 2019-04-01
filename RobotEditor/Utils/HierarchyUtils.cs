using BrightIdeasSoftware;
using Robot.Abstractions;
using Robot.Recordings;
using RobotEditor.Abstractions;
using RobotEditor.Hierarchy;
using RobotRuntime;
using RobotRuntime.Recordings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RobotEditor.Utils
{
    public class HierarchyUtils
    {
        public static void CreateColumns(TreeListView treeListView, IHierarchyNodeStringConverter HierarchyNodeStringConverter)
        {
            treeListView.CanExpandGetter = x => (x as HierarchyNode).Children.Count > 0;
            treeListView.ChildrenGetter = x => (x as HierarchyNode).Children;

            var nameColumn = new OLVColumn("Name", "Name");
            nameColumn.AspectGetter = x => HierarchyNodeStringConverter.ToString(x as HierarchyNode);

            nameColumn.ImageGetter += delegate (object x)
            {
                var imageListIndex = -1;
                var node = (HierarchyNode)x;
                imageListIndex = node.Recording != null ? 0 : imageListIndex;
                imageListIndex = node.Command != null ? 1 : imageListIndex;
                return imageListIndex;
            };

            nameColumn.RendererDelegate += delegate (EventArgs e, Graphics g, Rectangle r, object rowObject)
            {
                var h = rowObject as HierarchyNode;
                g.DrawLine(new Pen(Color.Black, 1), new Point(r.Left, r.Top), new Point(r.Right, r.Top));
                return true;
            };

            nameColumn.Width = treeListView.Width;

            treeListView.CellRendererGetter = (object rowObject, OLVColumn column) => new TreeRendererWithHighlight();
            treeListView.UseCellFormatEvents = true;

            treeListView.IsSimpleDragSource = true;
            treeListView.IsSimpleDropSink = true;

            treeListView.Columns.Add(nameColumn);
        }

        public static void OnNewUserCommandsAppeared(ICommandFactory CommandFactory, ContextMenuStrip contextMenuStrip, int createMenuItemIndex,
            TreeListView treeListView, BaseHierarchyManager baseRecordingManager)
        {
            contextMenuStrip.BeginInvokeIfCreated(new MethodInvoker(() =>
            {
                var createMenuItem = (ToolStripMenuItem)contextMenuStrip.Items[createMenuItemIndex];

                createMenuItem.DropDownItems.Clear();
                foreach (var name in CommandFactory.CommandNames)
                {
                    var item = new ToolStripMenuItem(name);
                    item.Click += (sender, eventArgs) =>
                    {
                        var newCommand = CommandFactory.Create(name);
                        if (treeListView.SelectedObject is HierarchyNode selectedNode)
                        {
                            if (selectedNode.Recording != null)
                                selectedNode.Recording.AddCommand(newCommand);
                            else if (selectedNode.Command != null)
                            {
                                var parentCommand = selectedNode.Command;
                                var recording = baseRecordingManager.GetRecordingFromCommand(parentCommand);
                                if (parentCommand.CanBeNested)
                                    recording.AddCommand(newCommand, parentCommand);
                                else
                                    recording.InsertCommandAfter(newCommand, parentCommand);
                            }
                            else
                                baseRecordingManager.LoadedRecordings.Last().AddCommand(newCommand);
                        }
                        else
                        {
                            if (baseRecordingManager.LoadedRecordings.Count >= 1)
                                baseRecordingManager.LoadedRecordings.Last().AddCommand(newCommand);
                            else
                                Logger.Log(LogType.Error, "Cannot create command since no Recordings are loaded in the Hierarchy.");
                        }
                    };
                    createMenuItem.DropDownItems.Add(item);
                }
            }));
        }

        public static HierarchyNode OnCommandAddedToRecording(List<HierarchyNode> nodes, Recording recording, Command parentCommand, Command command)
        {
            var parentNode = recording.Commands.GetNodeFromValue(command).parent;
            System.Diagnostics.Debug.Assert(parentNode.value == parentCommand, "parentCommand and parentNode missmatched");

            var recordingNode = nodes.FindRecursively(recording);

            var parentHierarchyNode = parentCommand == null ? recordingNode : recordingNode.GetNodeFromValue(parentNode.value);
            return AddCommandToParentRecursive(recording, command, parentHierarchyNode);
        }

        public static HierarchyNode AddCommandToParentRecursive(Recording recording, Command command, HierarchyNode parentHierarchyNode, int pos = -1)
        {
            var nodeToAdd = new HierarchyNode(command, parentHierarchyNode);

            if (pos == -1)
                parentHierarchyNode.Children.Add(nodeToAdd);
            else
                parentHierarchyNode.Children.Insert(pos, nodeToAdd);
            // TODOOOOOOO: Assert that commandNOde and nodeToAdd is equal
            var commandNode = recording.Commands.GetNodeFromValue(command);
            foreach (var childNode in commandNode)
                AddCommandToParentRecursive(recording, childNode.value, nodeToAdd);

            return nodeToAdd;
        }

        public static void OnCommandRemovedFromRecording(List<HierarchyNode> nodes, Recording recording, Command parentCommand, int commandIndex)
        {
            var recordingNode = nodes.FindRecursively(recording);
            var parentNode = parentCommand == null ? recordingNode : recordingNode.GetNodeFromValue(parentCommand);

            parentNode.Children.RemoveAt(commandIndex);
        }

        public static void OnCommandModifiedOnRecording(List<HierarchyNode> nodes, Recording recording, Command oldCommand, Command newCommand)
        {
            var recordingNode = nodes.FindRecursively(recording);
            var commandNode = recordingNode.GetNodeFromValue(oldCommand);

            commandNode.Update(newCommand);
        }

        // Will not work with multi dragging
        public static HierarchyNode OnCommandInsertedInRecording(List<HierarchyNode> nodes, Recording recording, Command parentCommand, Command command, int pos)
        {
            var recordingNode = nodes.FindRecursively(recording);
            var parentNode = parentCommand == null ? recordingNode : recordingNode.GetNodeFromValue(parentCommand);

            return AddCommandToParentRecursive(recording, command, parentNode, pos);
        }
    }
}
