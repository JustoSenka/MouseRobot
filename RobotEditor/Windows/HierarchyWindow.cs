#define ENABLE_UI_TESTING

using BrightIdeasSoftware;
using Robot;
using Robot.Abstractions;
using Robot.Recordings;
using RobotEditor.Abstractions;
using RobotEditor.Hierarchy;
using RobotEditor.Utils;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor
{
    public partial class HierarchyWindow : DockContent, IHierarchyWindow
    {
        public event Action<BaseHierarchyManager, object> OnSelectionChanged;
        private List<HierarchyNode> m_Nodes = new List<HierarchyNode>();

        private HierarchyNodeDropDetails DropDetails;
        private HierarchyNode m_HighlightedNode;

        private IHierarchyManager HierarchyManager;
        private ITestRunner TestRunner;
        private IAssetManager AssetManager;
        private ICommandFactory CommandFactory;
        private IHierarchyNodeStringConverter HierarchyNodeStringConverter;
        public HierarchyWindow(IHierarchyManager RecordingManager, ITestRunner TestRunner, IAssetManager AssetManager,
            IHierarchyNodeStringConverter HierarchyNodeStringConverter, ICommandFactory CommandFactory)
        {
            this.HierarchyManager = RecordingManager;
            this.TestRunner = TestRunner;
            this.AssetManager = AssetManager;
            this.HierarchyNodeStringConverter = HierarchyNodeStringConverter;
            this.CommandFactory = CommandFactory;

            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
            treeListView.Font = Fonts.Default;

            DropDetails = new HierarchyNodeDropDetails()
            {
                Owner = this,
                DragAndDropAccepted = DragAndDropAcceptedCallback,
                HierarchyManager = HierarchyManager as BaseHierarchyManager
            };

            RecordingManager.CommandAddedToRecording += OnCommandAddedToRecording;
            RecordingManager.CommandRemovedFromRecording += OnCommandRemovedFromRecording;
            RecordingManager.CommandModifiedOnRecording += OnCommandModifiedOnRecording;
            RecordingManager.CommandInsertedInRecording += OnCommandInsertedInRecording;

            RecordingManager.RecordingAdded += OnRecordingLoaded;
            RecordingManager.RecordingModified += OnRecordingModified;
            RecordingManager.RecordingRemoved += OnRecordingRemoved;
            RecordingManager.RecordingPositioningChanged += OnRecordingPositioningChanged;

            TestRunner.TestRunEnd += OnRecordingsFinishedRunning;
            TestRunner.TestData.CommandRunningCallback += OnCommandRunning;

            // subscribing for both treeListView and contextMenuStrip creation, since it's not clear which will be created first
            treeListView.HandleCreated += AddNewCommandsToCreateMenu;
            contextMenuStrip.HandleCreated += AddNewCommandsToCreateMenu;
            CommandFactory.NewUserCommands += AddNewCommandsToCreateMenu;

            treeListView.FormatCell += UpdateFontsTreeListView;
            HierarchyUtils.CreateColumns(treeListView, HierarchyNodeStringConverter);

            treeListView.HandleCreated += UpdateHierarchy;
        }

        private void AddNewCommandsToCreateMenu(object sender, EventArgs e)
        {
            AddNewCommandsToCreateMenu();
        }

        private void AddNewCommandsToCreateMenu()
        {
            if (CommandFactory == null || contextMenuStrip == null || treeListView == null || HierarchyManager == null)
            {
                Logger.Log(LogType.Error, "HierarchyWindow.AddNewCommandsToCreateMenu() was called to early. Creating commands will not be possible. Please report a bug.",
                    "CommandFactory " + (CommandFactory == null) + ", contextMenuStrip " + (contextMenuStrip == null) +
                    ", treeListView " + (treeListView == null) + ", TestFixture " + (HierarchyManager == null));
                return;
            }

            HierarchyUtils.OnNewUserCommandsAppeared(CommandFactory, contextMenuStrip, 8,
                treeListView, HierarchyManager as BaseHierarchyManager);
        }

        private void UpdateFontsTreeListView(object sender, FormatCellEventArgs e)
        {
            var node = e.Model as HierarchyNode;
            if (node == null)
                return;

            if (node.Recording != null)
            {
                if (node.Recording == HierarchyManager.ActiveRecording && node.Recording.IsDirty)
                    e.SubItem.Font = Fonts.ActiveAndDirtyRecording;//.AddFont(Fonts.ActiveRecording);
                else if (node.Recording == HierarchyManager.ActiveRecording)
                    e.SubItem.Font = Fonts.ActiveRecording;
                else if (node.Recording.IsDirty)
                    e.SubItem.Font = Fonts.DirtyRecording;//.AddFont(Fonts.DirtyRecording);
                else
                    e.SubItem.Font = Fonts.Default;
            }

            if (node.Command != null)
            {
                if (node == m_HighlightedNode)
                    e.SubItem.BackColor = SystemColors.Highlight;
            }
        }

        private void UpdateHierarchy(object sender, EventArgs args)
        {
            m_Nodes.Clear();

            foreach (var s in HierarchyManager.LoadedRecordings)
                m_Nodes.Add(new HierarchyNode(s, DropDetails));

            RefreshTreeListViewAsync(() => treeListView.ExpandAll());
        }

        private IAsyncResult RefreshTreeListViewAsync(Action callbackAfterRefresh = null)
        {
            return treeListView.BeginInvokeIfCreated(new MethodInvoker(() =>
            {
                treeListView.Roots = m_Nodes;

                for (int i = 0; i < treeListView.Items.Count; ++i)
                    treeListView.Items[i].ImageIndex = 0;

                treeListView.Refresh();

                callbackAfterRefresh?.Invoke();
            }));
        }

        private void OnNewUserCommandsAppeared()
        {
            var createMenuItem = (ToolStripMenuItem)contextMenuStrip.Items[8];

            createMenuItem.DropDownItems.Clear();
            foreach (var name in CommandFactory.CommandNames)
            {
                var item = new ToolStripMenuItem(name);
                item.Click += (sender, events) =>
                {
                    var command = CommandFactory.Create(name);
                    HierarchyManager.ActiveRecording.AddCommand(command);
                };
                createMenuItem.DropDownItems.Add(item);
            }
        }

        #region RecordingManager Callbacks

        private void OnRecordingLoaded(Recording recording)
        {
            var node = new HierarchyNode(recording, DropDetails);
            m_Nodes.Add(node);

            RefreshTreeListViewAsync(() =>
            {
                treeListView.SelectedObject = node;
                treeListView.Expand(node);
            });

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        private void OnRecordingModified(Recording recording)
        {
            var node = new HierarchyNode(recording, DropDetails);
            var index = recording.GetIndex(HierarchyManager);
            m_Nodes[index] = node;
            RefreshTreeListViewAsync();

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        private void OnRecordingRemoved(int index)
        {
            var oldSelectedObject = treeListView.SelectedObject;

            m_Nodes.RemoveAt(index);

            RefreshTreeListViewAsync(() =>
            {
                if (treeListView.SelectedObject != oldSelectedObject)
                    OnSelectionChanged?.Invoke((BaseHierarchyManager)HierarchyManager, null);
            });

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        private void OnRecordingPositioningChanged()
        {
            foreach (var recording in HierarchyManager.LoadedRecordings)
            {
                var index = m_Nodes.FindIndex(n => n.Recording == recording);
                m_Nodes.MoveBefore(index, recording.GetIndex(HierarchyManager));
            }

            RefreshTreeListViewAsync();
            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        private void OnCommandAddedToRecording(Recording recording, Command parentCommand, Command command)
        {
            var addedNode = HierarchyUtils.OnCommandAddedToRecording(m_Nodes, recording, parentCommand, command);
            addedNode.DropDetails = DropDetails;

            var postRefreshAction = (addedNode.Parent.Children.Count == 1) ? () => treeListView.Expand(addedNode.Parent) : default(Action);

            RefreshTreeListViewAsync(postRefreshAction);
        }

        private void OnCommandRemovedFromRecording(Recording recording, Command parentCommand, int commandIndex)
        {
            HierarchyUtils.OnCommandRemovedFromRecording(m_Nodes, recording, parentCommand, commandIndex);
            RefreshTreeListViewAsync();
        }

        private void OnCommandModifiedOnRecording(Recording recording, Command oldCommand, Command newCommand)
        {
            HierarchyUtils.OnCommandModifiedOnRecording(m_Nodes, recording, oldCommand, newCommand);
            RefreshTreeListViewAsync();
        }

        // Will not work with multi dragging
        private void OnCommandInsertedInRecording(Recording recording, Command parentCommand, Command command, int pos)
        {
            var node = HierarchyUtils.OnCommandInsertedInRecording(m_Nodes, recording, parentCommand, command, pos);
            RefreshTreeListViewAsync(() => treeListView.SelectedObject = node);
        }

        #endregion

        #region Context Menu Items
        private void setActiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedNode = treeListView.SelectedObject as HierarchyNode;
            if (selectedNode == null || selectedNode.Recording == null)
                return;

            HierarchyManager.ActiveRecording = selectedNode.Recording;
            RefreshTreeListViewAsync();
        }

        public void newRecordingToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            HierarchyManager.NewRecording();
            RefreshTreeListViewAsync();

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedNode = treeListView.SelectedObject as HierarchyNode;
            if (selectedNode == null || selectedNode.Recording == null)
                return;

            Process.Start("explorer.exe", "/select, " + selectedNode.Recording.Path);
        }

        public void duplicateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var selectedNode = treeListView.SelectedObject as HierarchyNode;
            if (selectedNode == null)
                return;

            if (selectedNode.Recording != null)
            {
                HierarchyManager.NewRecording(selectedNode.Recording);
                HierarchyManager.MoveRecordingAfter(HierarchyManager.LoadedRecordings.Count - 1, selectedNode.Recording.GetIndex(HierarchyManager));
            }
            else if (selectedNode.Command != null)
            {
                var recording = selectedNode.TopLevelNode.Recording;

                var node = recording.Commands.GetNodeFromValue(selectedNode.Command);
                var clone = recording.CloneCommandStub(selectedNode.Command);

                recording.AddCommandNode(clone, node.parent.value);
                recording.MoveCommandAfter(clone.value, selectedNode.Command);
            }

            RefreshTreeListViewAsync();
            treeListView.Focus();

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        public void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var selectedNode = treeListView.SelectedObject as HierarchyNode;
            if (selectedNode == null)
                return;

            if (selectedNode.Recording != null)
                HierarchyManager.RemoveRecording(selectedNode.Recording);
            else if (selectedNode.Command != null)
                HierarchyManager.GetRecordingFromCommand(selectedNode.Command).RemoveCommand(selectedNode.Command);

            RefreshTreeListViewAsync();

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }
        #endregion

        #region Menu Items (save recordings from MainForm)
        public void SaveAllRecordings()
        {
            foreach (var recording in HierarchyManager)
            {
                if (!recording.IsDirty)
                    continue;

                if (recording.Path != "")
                    HierarchyManager.SaveRecording(recording, recording.Path);
                else
                    SaveSelectedRecordingWithDialog(recording, updateUI: false);
            }

            RefreshTreeListViewAsync();
        }

        public void SaveSelectedRecordingWithDialog(Recording recording, bool updateUI = true)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.InitialDirectory = Paths.AssetsPath;
            saveDialog.Filter = string.Format("Mouse Robot File (*.{0})|*.{0}", FileExtensions.Recording);
            saveDialog.Title = "Select a path for recording to save.";
            saveDialog.FileName = recording.Name + FileExtensions.RecordingD;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                HierarchyManager.SaveRecording(recording, saveDialog.FileName);
                if (updateUI)
                    RefreshTreeListViewAsync();
            }
        }
        #endregion

        #region Drag & Drop

        private void treeListView_ModelCanDrop(object sender, ModelDropEventArgs e)
        {
            var targetNode = e.TargetModel as HierarchyNode;
            var sourceNode = e.SourceModels[0] as HierarchyNode;

            e.DropSink.CanDropBetween = true;

            if (targetNode == null || sourceNode == null ||
                targetNode.Recording == null && sourceNode.Command == null ||
                targetNode.Recording != null && sourceNode.Recording != null && e.DropTargetLocation == DropTargetLocation.Item ||
                sourceNode.Recording != null && sourceNode.DropDetails.Owner != this) // Do not let to drag recordings from other windows
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.DropSink.CanDropOnItem = targetNode.Recording != null || targetNode.Command.CanBeNested;

            if (targetNode.Recording != null && sourceNode.Command != null)
                e.DropSink.CanDropBetween = false;

            if (sourceNode.GetAllNodes().Contains(targetNode))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.Move;
        }

        private void treeListView_ModelDropped(object sender, ModelDropEventArgs e)
        {
            var targetNode = e.TargetModel as HierarchyNode;
            var sourceNode = e.SourceModels[0] as HierarchyNode;

            // Drag source and target are in same window
            if (sourceNode.DropDetails.Owner == this)
            {
                if (targetNode.Recording != null && sourceNode.Recording != null)
                {
                    if (e.DropTargetLocation == DropTargetLocation.AboveItem)
                        HierarchyManager.MoveRecordingBefore(sourceNode.Recording.GetIndex(HierarchyManager), targetNode.Recording.GetIndex(HierarchyManager));
                    if (e.DropTargetLocation == DropTargetLocation.BelowItem)
                        HierarchyManager.MoveRecordingAfter(sourceNode.Recording.GetIndex(HierarchyManager), targetNode.Recording.GetIndex(HierarchyManager));
                }

                if (targetNode.Command != null && sourceNode.Command != null)
                {
                    var targetRecording = HierarchyManager.GetRecordingFromCommand(targetNode.Command);
                    var sourceRecording = HierarchyManager.GetRecordingFromCommand(sourceNode.Command);

                    if (e.DropTargetLocation == DropTargetLocation.AboveItem)
                        HierarchyManager.MoveCommandBefore(sourceNode.Command, targetNode.Command, sourceRecording.GetIndex(HierarchyManager), targetRecording.GetIndex(HierarchyManager));
                    if (e.DropTargetLocation == DropTargetLocation.BelowItem)
                        HierarchyManager.MoveCommandAfter(sourceNode.Command, targetNode.Command, sourceRecording.GetIndex(HierarchyManager), targetRecording.GetIndex(HierarchyManager));

                    if (e.DropTargetLocation == DropTargetLocation.Item && targetNode.Command.CanBeNested)
                    {
                        var node = sourceRecording.Commands.GetNodeFromValue(sourceNode.Command);
                        sourceRecording.RemoveCommand(sourceNode.Command);
                        targetRecording.AddCommandNode(node, targetNode.Command);
                    }
                }

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

                if (targetNode.Command != null)
                {
                    sourceNode.DropDetails.DragAndDropAccepted?.Invoke(sourceNode);
                    var targetRecording = HierarchyManager.GetRecordingFromCommand(targetNode.Command);

                    if (e.DropTargetLocation == DropTargetLocation.AboveItem)
                        targetRecording.InsertCommandNodeBefore(node, targetNode.Command);

                    if (e.DropTargetLocation == DropTargetLocation.BelowItem)
                        targetRecording.InsertCommandNodeAfter(node, targetNode.Command);

                    if (e.DropTargetLocation == DropTargetLocation.Item && targetNode.Command.CanBeNested)
                        targetRecording.AddCommandNode(node, targetNode.Command);

                }

                if (targetNode.Recording != null)
                {
                    sourceNode.DropDetails.DragAndDropAccepted?.Invoke(sourceNode);
                    targetNode.Recording.AddCommandNode(node);
                }
            }
        }

        /// <summary>
        /// This callback is used when node is dropped in other window which knows nothing about this one
        /// </summary>
        private void DragAndDropAcceptedCallback(HierarchyNode node)
        {
            var sourceRecording = HierarchyManager.GetRecordingFromCommand(node.Command);
            sourceRecording.RemoveCommand(node.Command);
        }

        #endregion

        #region RecordingRunner Callbacks

        private void OnCommandRunning(Guid guid)
        {
            var recording = HierarchyManager.GetRecordingFromCommandGuid(guid);
            if (recording == null)
                return;

            var recordingNode = m_Nodes.FirstOrDefault(node => node.Recording == recording);
            if (recordingNode == null)
                return;

            var commandNode = recordingNode.GetNode(guid);

            m_HighlightedNode = commandNode;
            treeListView.BeginInvokeIfCreated(new MethodInvoker(() => treeListView.Refresh()));
        }

        private void OnRecordingsFinishedRunning()
        {
            m_HighlightedNode = null;

            treeListView.BeginInvokeIfCreated(new MethodInvoker(() => treeListView.Refresh()));
        }

        #endregion

        #region ToolStrip Buttons

        private void ToolstripExpandAll_Click(object sender, EventArgs e)
        {
            treeListView.ExpandAll();
        }

        private void ToolstripExpandOne_Click(object sender, EventArgs e)
        {
            treeListView.CollapseAll();
            foreach (var node in m_Nodes)
                treeListView.Expand(node);
        }

        private void ToolstripCollapseAll_Click(object sender, EventArgs e)
        {
            treeListView.CollapseAll();
        }

        #endregion

        public ToolStrip ToolStrip { get { return toolStrip; } }

        private void treeListView_SelectionChanged(object sender, EventArgs e)
        {
            if (!(treeListView.SelectedObject is HierarchyNode node))
            {
                OnSelectionChanged?.Invoke((BaseHierarchyManager)HierarchyManager, null);
            }
            else
            {
                OnSelectionChanged?.Invoke((BaseHierarchyManager)HierarchyManager, node.Value);
            }
        }

        private void TestFixtureWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            TestRunner.TestRunEnd -= OnRecordingsFinishedRunning;
            TestRunner.TestData.CommandRunningCallback -= OnCommandRunning;
            CommandFactory.NewUserCommands -= AddNewCommandsToCreateMenu;
            treeListView.HandleCreated -= AddNewCommandsToCreateMenu;
            contextMenuStrip.HandleCreated -= AddNewCommandsToCreateMenu;
            treeListView.FormatCell -= UpdateFontsTreeListView;
            treeListView.HandleCreated -= UpdateHierarchy;
        }

        private void ASSERT_TreeViewIsTheSameAsInRecordingManager()
        {
#if ENABLE_UI_TESTING
            for (int i = 0; i < m_Nodes.Count; i++)
            {
                Debug.Assert(m_Nodes[i].Recording == HierarchyManager.LoadedRecordings[i],
                    string.Format("Hierarchy recording missmatch: {0}:{1}", i, m_Nodes[i].Value.ToString()));

                // Will not work in nested scenarios
                for (int j = 0; j < m_Nodes[i].Recording.Commands.Count(); j++)
                {
                    Debug.Assert(m_Nodes[i].Children[j].Command == HierarchyManager.LoadedRecordings[i].Commands.GetChild(j).value,
                        string.Format("Hierarchy command missmatch: {0}:{1}, {2}:{3}",
                        i, m_Nodes[i].Value.ToString(), j, m_Nodes[i].Recording.Commands.GetChild(j).value.ToString()));
                }
            }
#endif
        }
    }
}
