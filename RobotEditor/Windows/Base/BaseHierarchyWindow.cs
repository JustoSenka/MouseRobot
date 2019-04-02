using BrightIdeasSoftware;
using Robot;
using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotEditor.Hierarchy;
using RobotEditor.Utils;
using RobotRuntime;
using RobotRuntime.Recordings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor.Windows.Base
{
    public class BaseHierarchyWindow : DockContent
    {
        protected new string Name;

        protected bool SuppressRefreshAndSelection = false;

        protected ToolStrip m_ToolStrip;
        protected TreeListView m_TreeListView;
        protected List<HierarchyNode> m_Nodes = new List<HierarchyNode>();
        protected IBaseHierarchyManager HierarchyManager;
        protected HierarchyNodeDropDetails DropDetails;
        protected HierarchyNode m_HighlightedNode;

        public event Action<IBaseHierarchyManager, object> OnSelectionChanged;
        protected void InvokeOnSelectionChanged(IBaseHierarchyManager HierarchyManager, object selectedObject) =>
            OnSelectionChanged?.Invoke(HierarchyManager, selectedObject);

        protected readonly ICommandFactory CommandFactory;
        public BaseHierarchyWindow() { }
        public BaseHierarchyWindow(ICommandFactory CommandFactory)
        {
            this.CommandFactory = CommandFactory;
        }

        #region Misc Important Methods

        protected void CreateDropDetails(IBaseHierarchyManager hierarchyManager)
        {
            DropDetails = new HierarchyNodeDropDetails()
            {
                Owner = this,
                DragAndDropAccepted = DragAndDropAcceptedCallback,
                HierarchyManager = hierarchyManager
            };
        }

        protected void AddNewCommandsToCreateMenu(ContextMenuStrip contextMenuStrip, TreeListView treeListView)
        {
            if (CommandFactory == null || contextMenuStrip == null || treeListView == null)
            {
                Logger.Log(LogType.Error, this.GetType().Name + "AddNewCommandsToCreateMenu() was called to early. Creating commands will not be possible. Please report a bug.",
                    "CommandFactory " + (CommandFactory == null) + ", contextMenuStrip " + (contextMenuStrip == null) +
                    ", treeListView " + (treeListView == null) + ", TestFixture " + (HierarchyManager == null));
                return;
            }

            if (HierarchyManager == null)
            {
                // Sometimes this will be called with HierarchyManager not assigned yet. It depends how fast ui elements are created.
                // So it is easier to just early return here. New user commands will be added upon other callback
                return;
            }

            OnNewUserCommandsAppeared(CommandFactory, contextMenuStrip, "createToolStripMenuItem", treeListView, HierarchyManager);
        }

        protected IAsyncResult RefreshTreeListViewAsync(Action callbackAfterRefresh = null)
        {
            return m_TreeListView.BeginInvokeIfCreated(new MethodInvoker(() =>
            {
                this.Text = Name;
                m_TreeListView.Roots = m_Nodes;

                for (int i = 0; i < m_TreeListView.Items.Count; ++i)
                {
                    m_TreeListView.Items[i].ImageIndex = 0;
                }
                m_TreeListView.Refresh();

                callbackAfterRefresh?.Invoke();
            }));
        }

        protected virtual void SubscribeAllEvents(IBaseHierarchyManager hierarchy)
        {
            hierarchy.CommandAddedToRecording += OnCommandAddedToRecording;
            hierarchy.CommandRemovedFromRecording += OnCommandRemovedFromRecording;
            hierarchy.CommandModifiedOnRecording += OnCommandModifiedOnRecording;
            hierarchy.CommandInsertedInRecording += OnCommandInsertedInRecording;

            hierarchy.RecordingAdded += OnRecordingLoaded;
            hierarchy.RecordingModified += OnRecordingModified;
            hierarchy.RecordingRemoved += OnRecordingRemoved;
            hierarchy.RecordingPositioningChanged += OnRecordingPositioningChanged;
        }

        protected virtual void UnsubscribeAllEvents(IBaseHierarchyManager hierarchy)
        {
            hierarchy.CommandAddedToRecording -= OnCommandAddedToRecording;
            hierarchy.CommandRemovedFromRecording -= OnCommandRemovedFromRecording;
            hierarchy.CommandModifiedOnRecording -= OnCommandModifiedOnRecording;
            hierarchy.CommandInsertedInRecording -= OnCommandInsertedInRecording;

            hierarchy.RecordingAdded -= OnRecordingLoaded;
            hierarchy.RecordingModified -= OnRecordingModified;
            hierarchy.RecordingRemoved -= OnRecordingRemoved;
            hierarchy.RecordingPositioningChanged -= OnRecordingPositioningChanged;
        }

        // TODO: Also mark parent commands/recordings/tests
        protected virtual void OnCommandRunning(Guid guid)
        {
            var recording = HierarchyManager.GetRecordingFromCommandGuid(guid);
            if (recording == null)
                return;

            var recordingNode = m_Nodes.FirstOrDefault(node => node.Recording == recording);
            if (recordingNode == null)
                return;

            var commandNode = recordingNode.GetNode(guid);

            m_HighlightedNode = commandNode;
            m_TreeListView.BeginInvokeIfCreated(new MethodInvoker(() => m_TreeListView.Refresh()));
        }

        protected virtual void OnRecordingsFinishedRunning()
        {
            m_HighlightedNode = null;
            m_TreeListView.BeginInvokeIfCreated(new MethodInvoker(() => m_TreeListView.Refresh()));
        }

        #endregion

        #region Context Menu Item Callbacks

        public virtual void newRecordingToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            HierarchyManager.NewRecording();
            RefreshTreeListViewAsync();

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        public virtual void duplicateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var selectedObjects = m_TreeListView.SelectedObjects.SafeCast<HierarchyNode>();
            if (IsMultiSelectionNotValid(selectedObjects))
                return;

            if (!IsMultiSelectionInOneBlock(selectedObjects))
                return;

            SuppressRefreshAndSelection = true;

            // Newly created commands will be placed after last node
            var lastNode = selectedObjects.Last();
            var lastNodeRecording = lastNode.Recording;
            var lastNodeCommand = lastNode.Command;

            // Selection will be moved to newly created nodes
            var parentNode = lastNode.Parent;
            var lastNodeIndex = parentNode.Children.IndexOf(lastNode);
            var objCount = m_TreeListView.SelectedObjects.Count;

            foreach (var selectedNode in selectedObjects)
            {
                if (selectedNode.Recording != null)
                {
                    var newRec = HierarchyManager.NewRecording(selectedNode.Recording);
                    HierarchyManager.MoveRecordingAfter(HierarchyManager.LoadedRecordings.Count - 1, lastNodeRecording.GetIndex(HierarchyManager));
                    lastNodeRecording = newRec;
                }
                else if (selectedNode.Command != null)
                {
                    var recording = selectedNode.TopLevelRecordingNode.Recording;

                    var node = recording.Commands.GetNodeFromValue(selectedNode.Command);
                    var clone = recording.CloneCommandStub(selectedNode.Command);

                    recording.AddCommandNode(clone, node.parent.value);
                    recording.MoveCommandAfter(clone.value, lastNodeCommand);
                    lastNodeCommand = clone.value;
                }
            }

            SuppressRefreshAndSelection = false;

            var nodesToSelect = parentNode.Children.Skip(lastNodeIndex + 1).Take(objCount);

            m_TreeListView.SelectedObjects = null;
            RefreshTreeListViewAsync(() => m_TreeListView.SelectObjects(nodesToSelect.ToList()));

            m_TreeListView.Focus();
            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        public virtual void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedObjects = m_TreeListView.SelectedObjects.SafeCast<HierarchyNode>();
            if (IsMultiSelectionNotValid(selectedObjects))
                return;

            var objCount = m_TreeListView.SelectedObjects.Count;
            foreach (var selectedNode in selectedObjects)
            {
                if (selectedNode.Recording != null)
                    HierarchyManager.RemoveRecording(selectedNode.Recording);

                else if (selectedNode.Command != null)
                {
                    var rec = HierarchyManager.GetRecordingFromCommand(selectedNode.Command);

                    // If command is nested under another command, it can already be deleted. In that case recording will not be found for that command
                    if (rec != null)
                        rec.RemoveCommand(selectedNode.Command);
                }
            }

            if (objCount > 1) // Of multiple objects were destroyed, deselect all. If only one, select subsequent element
                m_TreeListView.SelectedObjects = null;

            RefreshTreeListViewAsync();
            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        public virtual void treeListView_SelectionChanged(object sender, EventArgs e)
        {
            if (!(m_TreeListView.SelectedObject is HierarchyNode node))
                InvokeOnSelectionChanged(HierarchyManager, null);

            else
                InvokeOnSelectionChanged(HierarchyManager, node.Value);
        }


        #endregion

        #region Callbacks from IBaseHierarchyManager

        protected virtual void OnRecordingLoaded(Recording recording)
        {
            var node = new HierarchyNode(recording, DropDetails);
            m_Nodes.Add(node);

            if (!SuppressRefreshAndSelection)
            {
                RefreshTreeListViewAsync(() =>
                {
                    m_TreeListView.SelectedObject = node;
                    m_TreeListView.Expand(node);
                });
            }

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        protected virtual void OnRecordingModified(Recording recording)
        {
            var node = new HierarchyNode(recording, DropDetails);
            var index = recording.GetIndex(HierarchyManager);
            m_Nodes[index] = node;

            if (!SuppressRefreshAndSelection)
                RefreshTreeListViewAsync();

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        protected virtual void OnRecordingRemoved(int index)
        {
            var oldSelectedObject = m_TreeListView.SelectedObject;

            m_Nodes.RemoveAt(index);

            if (!SuppressRefreshAndSelection)
            {
                RefreshTreeListViewAsync(() =>
                {
                    if (m_TreeListView.SelectedObject != oldSelectedObject)
                        InvokeOnSelectionChanged(HierarchyManager, null);
                });
            }

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        protected virtual void OnRecordingPositioningChanged()
        {
            foreach (var recording in HierarchyManager.LoadedRecordings)
            {
                var index = m_Nodes.FindIndex(n => n.Recording == recording);
                m_Nodes.MoveBefore(index, recording.GetIndex(HierarchyManager));
            }

            if (!SuppressRefreshAndSelection)
                RefreshTreeListViewAsync();

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        protected virtual void OnCommandAddedToRecording(Recording recording, Command parentCommand, Command command)
        {
            var addedNode = BaseHierarchyWindow.OnCommandAddedToRecording(m_Nodes, recording, parentCommand, command);
            addedNode.DropDetails = DropDetails;

            var postRefreshAction = (addedNode.Parent.Children.Count == 1) ? () => m_TreeListView.Expand(addedNode.Parent) : default(Action);

            if (!SuppressRefreshAndSelection)
                RefreshTreeListViewAsync(postRefreshAction);
        }

        protected virtual void OnCommandRemovedFromRecording(Recording recording, Command parentCommand, int commandIndex)
        {
            OnCommandRemovedFromRecording(m_Nodes, recording, parentCommand, commandIndex);

            if (!SuppressRefreshAndSelection)
                RefreshTreeListViewAsync();
        }

        protected virtual void OnCommandModifiedOnRecording(Recording recording, Command oldCommand, Command newCommand)
        {
            OnCommandModifiedOnRecording(m_Nodes, recording, oldCommand, newCommand);

            if (!SuppressRefreshAndSelection)
                RefreshTreeListViewAsync();
        }

        // Will not work with multi dragging
        protected virtual void OnCommandInsertedInRecording(Recording recording, Command parentCommand, Command command, int pos)
        {
            var node = OnCommandInsertedInRecording(m_Nodes, recording, parentCommand, command, pos);

            if (!SuppressRefreshAndSelection)
                RefreshTreeListViewAsync(() => m_TreeListView.SelectedObject = node);
        }

        #endregion

        #region Drag & Drop

        /// <summary>
        /// Returns true if drop should not be accepted.
        /// Returns false if continue with drop
        /// </summary>
        protected virtual bool ShouldCancelDrop(HierarchyNode targetNode, HierarchyNode sourceNode, ModelDropEventArgs e)
        {
            return targetNode == null || sourceNode == null || // Any of nodes are null
                sourceNode.Recording == null && sourceNode.Command == null || // Source node is empty string value
                targetNode.Recording == null && targetNode.Command == null;   // Target node is empty string value
        }

        protected virtual void treeListView_ModelCanDrop(object sender, ModelDropEventArgs e)
        {
            var targetNode = e.TargetModel as HierarchyNode;
            var sourceNodes = e.SourceModels.SafeCast<HierarchyNode>();

            var shouldCancel = IsMultiSelectionNotValid(sourceNodes) // Do not allow any null or invalid values to be dragged on this window
                || !IsMultiSelectionInOneBlock(sourceNodes) // Only allow one block to be dragged
                || sourceNodes.Any(n => ShouldCancelDrop(targetNode, n, e) // User defined specific window condition
                || n.GetAllNodes(false).Contains(n)); // Cannot drop node inside itself

            if (shouldCancel)
            {
                e.Effect = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            // All objects are of same type so checking first one should be ok. 
            var amIDraggingCommands = sourceNodes.First().Command != null;

            e.DropSink.CanDropOnItem = amIDraggingCommands && // We don't want recordings to be dropped on any item
                    (targetNode.Recording != null || targetNode.Command.CanBeNested); // Everything can be dropped on recording and commands with nested tag can also be dropped onto

            // Cannot drop commands inbetween recordings
            e.DropSink.CanDropBetween = !(targetNode.Recording != null && amIDraggingCommands);

            e.Effect = DragDropEffects.Move;
            e.Handled = true;
        }

        protected virtual void treeListView_ModelDropped(object sender, ModelDropEventArgs e)
        {
            var targetNode = e.TargetModel as HierarchyNode;
            var sourceNodes = e.SourceModels.SafeCast<HierarchyNode>();

            SuppressRefreshAndSelection = true;
            foreach (var sourceNode in sourceNodes)
            {
                // Drag source and target are in same window
                if (sourceNode.DropDetails.Owner == this)
                {
                    // Moving recording between other recordings
                    if (targetNode.Recording != null && sourceNode.Recording != null)
                        HierarchyManager.MoveRecording(e.DropTargetLocation,
                            sourceNode.Recording.GetIndex(HierarchyManager),
                            targetNode.Recording.GetIndex(HierarchyManager));

                    // Moving command between or onto other commands
                    if (targetNode.Command != null && sourceNode.Command != null)
                        HierarchyManager.MoveCommand(e.DropTargetLocation, sourceNode.Command, targetNode.Command);

                    // Moving command on top of recording
                    if (targetNode.Recording != null && sourceNode.Command != null)
                    {
                        var sourceRecording = HierarchyManager.GetRecordingFromCommand(sourceNode.Command);

                        var node = sourceRecording.Commands.GetNodeFromValue(sourceNode.Command);
                        sourceRecording.RemoveCommand(sourceNode.Command);
                        targetNode.Recording.AddCommandNode((TreeNode<Command>)node);
                    }
                }
                else // Drag source come from different window
                {
                    if (sourceNode.Recording != null) // Do not allow recordings
                        return;

                    var sourceRecording = sourceNode.DropDetails.HierarchyManager.GetRecordingFromCommand(sourceNode.Command);
                    var node = sourceRecording.Commands.GetNodeFromValue(sourceNode.Command);

                    // Since we're reusing the same command node instance, first remove from old recodring
                    sourceNode.DropDetails.DragAndDropAccepted?.Invoke(sourceNode);

                    // Moving command between or onto other commands
                    if (targetNode.Command != null)
                        HierarchyManager.InsertCommandNode(e.DropTargetLocation, node, targetNode.Command);

                    // Moving command on top of recording
                    if (targetNode.Recording != null)
                        targetNode.Recording.AddCommandNode(node);
                }
            }
            SuppressRefreshAndSelection = false;

            // Selection will be moved to newly created nodes
            // Those nodes should appear below/above target node
            // If target node is script/nested command and dropped on Item, selection should go to last nodes
            var parentNode = e.DropTargetLocation == DropTargetLocation.Item ? targetNode : targetNode.Parent;
            var targetNodeIndex = parentNode.Children.IndexOf(targetNode);
            var objCount = e.SourceModels.Count;

            IEnumerable<HierarchyNode> nodesToSelect = null;
            if (e.DropTargetLocation == DropTargetLocation.AboveItem)
                nodesToSelect = parentNode.Children.Skip(targetNodeIndex - objCount).Take(objCount);

            else if (e.DropTargetLocation == DropTargetLocation.BelowItem)
                nodesToSelect = parentNode.Children.Skip(targetNodeIndex + 1).Take(objCount);

            else if (e.DropTargetLocation == DropTargetLocation.Item)
                nodesToSelect = parentNode.Children.Skip(parentNode.Children.Count - objCount).Take(objCount);

            m_TreeListView.SelectedObjects = null;
            m_TreeListView.Focus();
            RefreshTreeListViewAsync(() => 
            {
                m_TreeListView.Expand(parentNode);
                if (nodesToSelect != null)
                    m_TreeListView.SelectObjects(nodesToSelect.ToList());
            });
        }

        /// <summary>
        /// This callback is used when node is dropped in other window which knows nothing about this one
        /// </summary>
        protected virtual void DragAndDropAcceptedCallback(HierarchyNode node)
        {
            var sourceRecording = HierarchyManager.GetRecordingFromCommand(node.Command);
            sourceRecording.RemoveCommand(node.Command);
            m_TreeListView.SelectedObjects = null;
        }

        #endregion

        #region ToolStrip Buttons

        public virtual void ToolstripExpandAll_Click(object sender, EventArgs e)
        {
            m_TreeListView.ExpandAll();
        }

        public virtual void ToolstripExpandOne_Click(object sender, EventArgs e)
        {
            m_TreeListView.CollapseAll();
            foreach (var node in m_Nodes)
                m_TreeListView.Expand(node);
        }

        public virtual void ToolstripCollapseAll_Click(object sender, EventArgs e)
        {
            m_TreeListView.CollapseAll();
        }

        public ToolStrip ToolStrip { get { return m_ToolStrip; } }

        #endregion

        #region Static helpers

        /// <summary>
        /// Returns true if both commands and recordings are selected
        /// </summary>
        public static bool IsMultiSelectionNotValid(IEnumerable<HierarchyNode> selectedObjects)
        {
            var nodeList = selectedObjects.Cast<object>().Select(o => o as HierarchyNode);
            var areAnyNulls = nodeList.Any(n => n == null);
            if (areAnyNulls)
                return false;

            var commandCount = nodeList.Count(n => n.Command != null);
            var recordingCount = nodeList.Count(n => n.Recording != null);

            return commandCount > 0 && recordingCount > 0 || commandCount == 0 && recordingCount == 0;
        }

        /// <summary>
        /// Returns true if all nodes are on same level and next to each other
        /// </summary>
        public static bool IsMultiSelectionInOneBlock(IEnumerable<HierarchyNode> selectedObjects)
        {
            var firstCommandLevel = selectedObjects.First().Level;
            var areOnSameLevel = selectedObjects.Any(n => n.Level == firstCommandLevel);

            if (!areOnSameLevel)
                return false;

            var parent = selectedObjects.First().Parent;
            var indices = selectedObjects.Select(n => parent.Children.IndexOf(n));
            if (!indices.AreNumbersConsecutive())
                return false;

            return true;
        }

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

        public static void OnNewUserCommandsAppeared(ICommandFactory CommandFactory, ContextMenuStrip contextMenuStrip, string menuItemName,
            TreeListView treeListView, IBaseHierarchyManager baseRecordingManager)
        {
            contextMenuStrip.BeginInvokeIfCreated(new MethodInvoker(() =>
            {
                var createMenuItem = (ToolStripMenuItem)contextMenuStrip.Items.Find(menuItemName, true)[0];

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
            // ? Assert that commandNOde and nodeToAdd is equal
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

        #endregion

        protected virtual void ASSERT_TreeViewIsTheSameAsInRecordingManager() { }
    }
}
