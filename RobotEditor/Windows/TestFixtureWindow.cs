#define ENABLE_UI_TESTING

using BrightIdeasSoftware;
using Robot.Abstractions;
using Robot.Recordings;
using RobotEditor.Abstractions;
using RobotEditor.Hierarchy;
using RobotEditor.Utils;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
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
    public partial class TestFixtureWindow : DockContent, ITestFixtureWindow
    {
        public event Action<BaseHierarchyManager, object> OnSelectionChanged;
        private List<HierarchyNode> m_Nodes = new List<HierarchyNode>();

        private HierarchyNodeDropDetails DropDetails;
        private HierarchyNode m_HighlightedNode;

        private TestFixture m_TestFixture;
        private HierarchyNode m_HooksNode;
        private HierarchyNode m_TestsNode;

        private ITestRunner TestRunner;
        private ITestFixtureManager TestFixtureManager;
        private IHierarchyNodeStringConverter HierarchyNodeStringConverter;
        private ICommandFactory CommandFactory;
        public TestFixtureWindow(ITestRunner TestRunner, ITestFixtureManager TestFixtureManager,
            IHierarchyNodeStringConverter HierarchyNodeStringConverter, ICommandFactory CommandFactory)
        {
            this.TestRunner = TestRunner;
            this.TestFixtureManager = TestFixtureManager;
            this.HierarchyNodeStringConverter = HierarchyNodeStringConverter;
            this.CommandFactory = CommandFactory;

            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
            treeListView.Font = Fonts.Default;

            DropDetails = new HierarchyNodeDropDetails()
            {
                Owner = this,
                DragAndDropAccepted = DragAndDropAcceptedCallback,
                HierarchyManager = m_TestFixture as BaseHierarchyManager
            };

            TestRunner.TestRunEnd += OnRecordingsFinishedRunning;
            TestRunner.TestData.CommandRunningCallback += OnCommandRunning;

            treeListView.FormatCell += UpdateFontsTreeListView;
            HierarchyUtils.CreateColumns(treeListView, HierarchyNodeStringConverter);

            // subscribing for both treeListView and contextMenuStrip creation, since it's not clear which will be created first
            treeListView.HandleCreated += AddNewCommandsToCreateMenu;
            contextMenuStrip.HandleCreated += AddNewCommandsToCreateMenu;
            CommandFactory.NewUserCommands += AddNewCommandsToCreateMenu;

            UpdateHierarchy();
        }

        private void AddNewCommandsToCreateMenu(object sender, EventArgs e)
        {
            AddNewCommandsToCreateMenu();
        }

        private void AddNewCommandsToCreateMenu()
        {
            if (CommandFactory == null || contextMenuStrip == null || treeListView == null)
            {
                Logger.Log(LogType.Error, "TestFixtureWindow.AddNewCommandsToCreateMenu() was called to early. Creating commands will not be possible. Please report a bug.",
                    "CommandFactory " + (CommandFactory == null) + ", contextMenuStrip " + (contextMenuStrip == null) +
                    ", treeListView " + (treeListView == null) + ", TestFixture " + (m_TestFixture == null));
                return;
            }

            if (m_TestFixture == null)
            {
                // Sometimes this will be called with m_TestFixture not assigned yet. It depends how fast ui elements are created.
                // So it is easier to just early return here. New user commands will be added upon other callback
                return;
            }

            HierarchyUtils.OnNewUserCommandsAppeared(CommandFactory, contextMenuStrip, 5,
                treeListView, m_TestFixture);
        }

        public void DisplayTestFixture(TestFixture fixture)
        {
            if (fixture == null || m_TestFixture == fixture)
                return;

            if (m_TestFixture != null)
                UnsubscribeAllEvents(m_TestFixture);

            m_TestFixture = fixture;
            DropDetails.HierarchyManager = fixture;
            SubscribeAllEvents(m_TestFixture);

            //AddNewCommandsToCreateMenu is magic. Uses m_TestFixture and creates lamda callbacks using it, whenever we change m_TestFixture, resubscribe this method
            CommandFactory.NewUserCommands -= AddNewCommandsToCreateMenu;
            CommandFactory.NewUserCommands += AddNewCommandsToCreateMenu;
            AddNewCommandsToCreateMenu();

            UpdateHierarchy();
        }

        private void SubscribeAllEvents(TestFixture fixture)
        {
            fixture.CommandAddedToRecording += OnCommandAddedToRecording;
            fixture.CommandRemovedFromRecording += OnCommandRemovedFromRecording;
            fixture.CommandModifiedOnRecording += OnCommandModifiedOnRecording;
            fixture.CommandInsertedInRecording += OnCommandInsertedInRecording;

            fixture.RecordingAdded += OnRecordingLoaded;
            fixture.RecordingModified += OnRecordingModified;
            fixture.RecordingRemoved += OnRecordingRemoved;
            fixture.RecordingPositioningChanged += OnRecordingPositioningChanged;
        }

        private void UnsubscribeAllEvents(TestFixture fixture)
        {
            fixture.CommandAddedToRecording -= OnCommandAddedToRecording;
            fixture.CommandRemovedFromRecording -= OnCommandRemovedFromRecording;
            fixture.CommandModifiedOnRecording -= OnCommandModifiedOnRecording;
            fixture.CommandInsertedInRecording -= OnCommandInsertedInRecording;

            fixture.RecordingAdded -= OnRecordingLoaded;
            fixture.RecordingModified -= OnRecordingModified;
            fixture.RecordingRemoved -= OnRecordingRemoved;
            fixture.RecordingPositioningChanged -= OnRecordingPositioningChanged;
        }

        private void UpdateFontsTreeListView(object sender, FormatCellEventArgs e)
        {
            if (!(e.Model is HierarchyNode node))
                return;

            if (node.Recording != null)
            {
                if (node.Recording.IsDirty)
                    e.SubItem.Font = Fonts.DirtyRecordingBold;
                else
                    e.SubItem.Font = Fonts.DefaultBold;
            }

            if (node.Command != null)
            {
                if (node == m_HighlightedNode)
                    e.SubItem.BackColor = SystemColors.Highlight;

                if (node.Command.GetType() == typeof(CommandUnknown))
                    e.SubItem.ForeColor = StandardColors.Red;
            }
        }

        private void UpdateHierarchy()
        {
            m_Nodes.Clear();

            if (m_TestFixture == null)
                return;

            m_HooksNode = new HierarchyNode("Special Recordings", DropDetails);
            foreach (var s in m_TestFixture.Hooks)
                m_HooksNode.AddHierarchyNode(new HierarchyNode(s, DropDetails));

            m_TestsNode = new HierarchyNode("Tests", DropDetails);
            foreach (var s in m_TestFixture.Tests)
                m_TestsNode.AddHierarchyNode(new HierarchyNode(s, DropDetails));
            m_Nodes.Add(m_HooksNode);
            m_Nodes.Add(m_TestsNode);

            RefreshTreeListViewAsync(() => treeListView.ExpandAll());
        }

        private IAsyncResult RefreshTreeListViewAsync(Action callbackAfterRefresh = null)
        {
            return treeListView.BeginInvokeIfCreated(new MethodInvoker(() =>
            {
                this.Text = m_TestFixture.ToString();
                treeListView.Roots = m_Nodes;

                for (int i = 0; i < treeListView.Items.Count; ++i)
                {
                    treeListView.Items[i].ImageIndex = 0;
                }
                treeListView.Refresh();

                callbackAfterRefresh?.Invoke();
            }));
        }

        #region RecordingManager Callbacks

        private void OnRecordingLoaded(Recording recording)
        {
            var node = new HierarchyNode(recording, DropDetails);

            m_TestsNode.AddHierarchyNode(node);
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
            m_Nodes.ReplaceNodeWithNewOne(node);

            RefreshTreeListViewAsync();

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        private void OnRecordingRemoved(int index)
        {
            var oldSelectedObject = treeListView.SelectedObject;

            m_Nodes.RemoveAtIndexRemoving4(index);
            RefreshTreeListViewAsync(() =>
            {
                if (treeListView.SelectedObject != oldSelectedObject)
                    OnSelectionChanged?.Invoke(m_TestFixture, null);
            });

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        private void OnRecordingPositioningChanged()
        {
            foreach (var recording in m_TestFixture.Tests)
            {
                var index = m_TestsNode.Children.FindIndex(n => n.Recording == recording);
                var indexInBackend = m_TestFixture.GetRecordingIndex(recording) - 4; // -4 since 4 recordings are reserved as hooks
                m_TestsNode.Children.MoveBefore(index, indexInBackend);
            }

            // Hooks do not allow drag and drop, but keeping here in case of position changing from recording
            foreach (var recording in m_TestFixture.Hooks)
            {
                var index = m_HooksNode.Children.FindIndex(n => n.Recording == recording);
                m_HooksNode.Children.MoveBefore(index, m_TestFixture.GetRecordingIndex(recording));
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

        private void newRecordingToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            m_TestFixture.NewRecording();
            RefreshTreeListViewAsync(() => treeListView.Expand(m_TestsNode));

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        private void duplicateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(treeListView.SelectedObject is HierarchyNode selectedNode))
                return;

            if (selectedNode.Recording != null && IsItASpecialRecording(selectedNode.Recording))
            {
                m_TestFixture.NewRecording(selectedNode.Recording);
                m_TestFixture.MoveRecordingAfter(m_TestFixture.LoadedRecordings.Count - 1, m_TestFixture.GetRecordingIndex(selectedNode.Recording));
            }
            else if (selectedNode.Command != null)
            {
                var recording = selectedNode.TopLevelRecordingNode.Recording;

                var node = recording.Commands.GetNodeFromValue(selectedNode.Command);
                var clone = recording.CloneCommandStub(selectedNode.Command);

                recording.AddCommandNode(clone, node.parent.value);
                recording.MoveCommandAfter(clone.value, selectedNode.Command);
            }

            RefreshTreeListViewAsync();
            treeListView.Focus();

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var selectedNode = treeListView.SelectedObject as HierarchyNode;
            if (selectedNode == null)
                return;

            if (selectedNode.Recording != null && IsItASpecialRecording(selectedNode.Recording))
                m_TestFixture.RemoveRecording(selectedNode.Recording);
            else if (selectedNode.Command != null)
                m_TestFixture.GetRecordingFromCommand(selectedNode.Command).RemoveCommand(selectedNode.Command);

            RefreshTreeListViewAsync();

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }
        #endregion

        #region Menu Items (save recordings from MainForm)
        public void SaveTestFixture()
        {
            if (!m_TestFixture.IsDirty)
                return;

            if (m_TestFixture.Path != "")
                TestFixtureManager.SaveTestFixture(m_TestFixture, m_TestFixture.Path);
            else
                SaveFixtureWithDialog(false);

            RefreshTreeListViewAsync();
        }

        public void SaveFixtureWithDialog(bool updateUI = true)
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                InitialDirectory = Paths.AssetsPath,
                Filter = string.Format("Test Fixture File (*.{0})|*.{0}", FileExtensions.Test),
                Title = "Select a path for recording to save.",
                FileName = m_TestFixture.Name + FileExtensions.TestD
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                TestFixtureManager.SaveTestFixture(m_TestFixture, saveDialog.FileName);
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
                sourceNode.Recording == null && sourceNode.Command == null || // Source node is empty string value
                targetNode.Recording == null && targetNode.Command == null || // Target node is empty string value
                targetNode.Recording == null && sourceNode.Command == null || // Cannot drag recordings onto commands
                m_HooksNode.Children.Contains(sourceNode) || // Hooks recordings are special and should not be moved at all
                m_HooksNode.Children.Contains(targetNode) && sourceNode.Recording != null || // Cannot drag any recording onto or inbetween hooks recordings
                targetNode.Recording != null && sourceNode.Recording != null && e.DropTargetLocation == DropTargetLocation.Item || // Cannot drag recordings onto recordings
                sourceNode.Recording != null && sourceNode.DropDetails.Owner != this) // Do not allow recordings from other windows
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.DropSink.CanDropOnItem = sourceNode.Recording == null && // We don't want recordings to be dropped on any item
                (targetNode.Recording != null || targetNode.Command.CanBeNested); // Everything can be dropped on recording and commands with nested tag can also be dropped onto

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
                        m_TestFixture.MoveRecordingBefore(m_TestFixture.GetRecordingIndex(sourceNode.Recording), m_TestFixture.GetRecordingIndex(targetNode.Recording));
                    if (e.DropTargetLocation == DropTargetLocation.BelowItem)
                        m_TestFixture.MoveRecordingAfter(m_TestFixture.GetRecordingIndex(sourceNode.Recording), m_TestFixture.GetRecordingIndex(targetNode.Recording));
                }

                if (targetNode.Command != null && sourceNode.Command != null)
                {
                    var targetRecording = m_TestFixture.GetRecordingFromCommand(targetNode.Command);
                    var sourceRecording = m_TestFixture.GetRecordingFromCommand(sourceNode.Command);

                    if (e.DropTargetLocation == DropTargetLocation.AboveItem)
                        m_TestFixture.MoveCommandBefore(sourceNode.Command, targetNode.Command, m_TestFixture.GetRecordingIndex(sourceRecording), m_TestFixture.GetRecordingIndex(targetRecording));
                    if (e.DropTargetLocation == DropTargetLocation.BelowItem)
                        m_TestFixture.MoveCommandAfter(sourceNode.Command, targetNode.Command, m_TestFixture.GetRecordingIndex(sourceRecording), m_TestFixture.GetRecordingIndex(targetRecording));

                    if (e.DropTargetLocation == DropTargetLocation.Item && targetNode.Command.CanBeNested)
                    {
                        var node = sourceRecording.Commands.GetNodeFromValue(sourceNode.Command);
                        sourceRecording.RemoveCommand(sourceNode.Command);
                        targetRecording.AddCommandNode(node, targetNode.Command);
                    }
                }

                if (targetNode.Recording != null && sourceNode.Command != null)
                {
                    var sourceRecording = m_TestFixture.GetRecordingFromCommand(sourceNode.Command);

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

                    var targetRecording = m_TestFixture.GetRecordingFromCommand(targetNode.Command);

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
            var sourceRecording = m_TestFixture.GetRecordingFromCommand(node.Command);
            sourceRecording.RemoveCommand(node.Command);
        }

        #endregion

        #region RecordingRunner Callbacks

        // TODO: Also mark parent commands/recordings/tests
        private void OnCommandRunning(Guid guid)
        {
            var recording = m_TestFixture.GetRecordingFromCommandGuid(guid);
            if (recording == null)
                return;

            var commandNode = m_Nodes.Select(node => node.GetNode(guid)).FirstOrDefault(node => node != null);
            if (commandNode == null)
                return;

            m_HighlightedNode = commandNode;
            treeListView.BeginInvokeIfCreated(new MethodInvoker(() => treeListView.Refresh()));
        }

        private void OnRecordingsFinishedRunning()
        {
            m_HighlightedNode = null;

            if (treeListView.Created)
                treeListView.Invoke(new Action(() => treeListView.Refresh()));
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
                OnSelectionChanged?.Invoke(m_TestFixture, null);
            }
            else
            {
                OnSelectionChanged?.Invoke(m_TestFixture, node.Value);
            }
        }

        private void TestFixtureWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            TestFixtureManager.Remove(m_TestFixture);

            UnsubscribeAllEvents(m_TestFixture);

            TestRunner.TestRunEnd -= OnRecordingsFinishedRunning;
            TestRunner.TestData.CommandRunningCallback -= OnCommandRunning;
            CommandFactory.NewUserCommands -= AddNewCommandsToCreateMenu;
            treeListView.HandleCreated -= AddNewCommandsToCreateMenu;
            contextMenuStrip.HandleCreated -= AddNewCommandsToCreateMenu;
            treeListView.FormatCell -= UpdateFontsTreeListView;
        }

        private bool IsItASpecialRecording(Recording recording)
        {
            if (recording == null)
                return false;

            return m_TestFixture.GetRecordingIndex(recording) >= 4;
        }

        private void ASSERT_TreeViewIsTheSameAsInRecordingManager()
        {
#if ENABLE_UI_TESTING

            var list = new List<HierarchyNode>();
            foreach (var n in m_Nodes[0].Children)
                list.Add(n);
            foreach (var n in m_Nodes[1].Children)
                list.Add(n);

            for (int i = 0; i < list.Count; i++)
            {
                Debug.Assert(list[i].Recording == m_TestFixture.LoadedRecordings[i],
                    string.Format("Hierarchy recording missmatch: {0}:{1}", i, list[i].Value.ToString()));

                // Will not work in nested scenarios
                for (int j = 0; j < list[i].Recording.Commands.Count(); j++)
                {
                    Debug.Assert(list[i].Children[j].Command == m_TestFixture.LoadedRecordings[i].Commands.GetChild(j).value,
                        string.Format("Hierarchy command missmatch: {0}:{1}, {2}:{3}",
                        i, list[i].Value.ToString(), j, list[i].Recording.Commands.GetChild(j).value.ToString()));
                }
            }
#endif
        }
    }
}
