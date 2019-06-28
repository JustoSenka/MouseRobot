#define ENABLE_UI_TESTING

using BrightIdeasSoftware;
using Robot.Abstractions;
using Robot.Tests;
using RobotEditor.Abstractions;
using RobotEditor.Hierarchy;
using RobotEditor.Windows.Base;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RobotEditor
{
    [RegisterTypeToContainer(typeof(ITestFixtureWindow))]
    public partial class TestFixtureWindow : BaseHierarchyWindow, ITestFixtureWindow
    {
        private TestFixture m_TestFixture;
        private HierarchyNode m_HooksNode;
        private HierarchyNode m_TestsNode;

        private readonly ITestRunner TestRunner;
        private readonly ITestFixtureManager TestFixtureManager;
        private readonly IHierarchyNodeStringConverter HierarchyNodeStringConverter;
        public TestFixtureWindow(ITestRunner TestRunner, ITestFixtureManager TestFixtureManager,
            IHierarchyNodeStringConverter HierarchyNodeStringConverter, ICommandFactory CommandFactory, IProfiler Profiler) : base(CommandFactory, Profiler)
        {
            this.TestRunner = TestRunner;
            this.TestFixtureManager = TestFixtureManager;
            this.HierarchyNodeStringConverter = HierarchyNodeStringConverter;

            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
            treeListView.Font = Fonts.Default;

            CreateDropDetails(HierarchyManager);
            base.m_ToolStrip = toolStrip;
            base.m_TreeListView = this.treeListView;

            TestRunner.TestRunEnd += OnRecordingsFinishedRunning;
            TestRunner.TestData.CommandRunningCallback += OnCommandRunning;

            treeListView.FormatCell += UpdateFontsTreeListView;
            BaseHierarchyWindow.CreateColumns(treeListView, HierarchyNodeStringConverter);

            // subscribing for both treeListView and contextMenuStrip creation, since it's not clear which will be created first
            treeListView.HandleCreated += AddNewCommandsToCreateMenu;
            contextMenuStrip.HandleCreated += AddNewCommandsToCreateMenu;
            CommandFactory.NewUserCommands += AddNewCommandsToCreateMenu;

            TestFixtureManager.FixtureRemoved += OnFixtureRemoved;

            UpdateHierarchy();
        }

        private void AddNewCommandsToCreateMenu(object sender, EventArgs e) =>
            AddNewCommandsToCreateMenu(contextMenuStrip, treeListView);

        private void AddNewCommandsToCreateMenu() =>
            AddNewCommandsToCreateMenu(contextMenuStrip, treeListView);

        public void DisplayTestFixture(TestFixture fixture)
        {
            if (fixture == null || m_TestFixture == fixture)
                return;

            if (m_TestFixture != null)
                UnsubscribeAllEvents(m_TestFixture);

            m_TestFixture = fixture;
            base.Name = fixture.Name;
            base.HierarchyManager = fixture;
            DropDetails.HierarchyManager = fixture;
            SubscribeAllEvents(m_TestFixture);

            //AddNewCommandsToCreateMenu is magic. Uses m_TestFixture and creates lamda callbacks using it, whenever we change m_TestFixture, resubscribe this method
            CommandFactory.NewUserCommands -= AddNewCommandsToCreateMenu;
            CommandFactory.NewUserCommands += AddNewCommandsToCreateMenu;
            AddNewCommandsToCreateMenu();

            UpdateHierarchy();
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

        #region Callbacks from IBaseHierarchyManager

        protected override void OnRecordingLoaded(Recording recording)
        {
            var node = new HierarchyNode(recording, DropDetails);

            var nodeToAddRec = LightTestFixture.IsSpecialRecording(recording) ? m_HooksNode : m_TestsNode;
            nodeToAddRec.AddHierarchyNode(node);

            RefreshTreeListViewAsync(() =>
            {
                treeListView.SelectedObject = node;
                treeListView.Expand(node);
            });

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        protected override void OnRecordingModified(Recording recording)
        {
            // TODO: Is there any reson this one is overriden and cannot be the same as in base class?
            var node = new HierarchyNode(recording, DropDetails);
            m_Nodes.ReplaceNodeWithNewOne(node);

            RefreshTreeListViewAsync();

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        protected override void OnRecordingRemoved(int index)
        {
            var oldSelectedObject = treeListView.SelectedObject;

            m_Nodes.RemoveAtIndexRemoving4(index);
            RefreshTreeListViewAsync(() =>
            {
                if (treeListView.SelectedObject != oldSelectedObject)
                    InvokeOnSelectionChanged(m_TestFixture, null);
            });

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        protected override void OnRecordingPositioningChanged()
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

        #endregion

        #region Context Menu Items

        public override void newRecordingToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            m_TestFixture.NewRecording();
            RefreshTreeListViewAsync(() => treeListView.Expand(m_TestsNode));

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        public override void duplicateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var areThereNullsOrSpecialScriptsSelected = treeListView.SelectedObjects
                .SafeCast<HierarchyNode>().Any(n => n == null || n.Recording != null
                && LightTestFixture.IsSpecialRecording(n.Recording));

            // Do not allow to duplicate special recordings
            if (areThereNullsOrSpecialScriptsSelected)
                return;

            base.duplicateToolStripMenuItem1_Click(sender, e);
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
                base.Name = m_TestFixture.Name;

                if (updateUI)
                    RefreshTreeListViewAsync();
            }
        }
        #endregion

        #region Drag & Drop

        /// <summary>
        /// Return true if drop should not be accepted.
        /// Return false if continue with drop
        /// </summary>
        protected override bool ShouldCancelDrop(HierarchyNode targetNode, HierarchyNode sourceNode, ModelDropEventArgs e)
        {
            var cancel = base.ShouldCancelDrop(targetNode, sourceNode, e);
            return cancel ||
                targetNode.Recording == null && sourceNode.Command == null || // Cannot drag recordings onto commands
                m_HooksNode.Children.Contains(sourceNode) || // Hooks recordings are special and should not be moved at all
                m_HooksNode.Children.Contains(targetNode) && sourceNode.Recording != null || // Cannot drag any recording onto or inbetween hooks recordings
                targetNode.Recording != null && sourceNode.Recording != null && e.DropTargetLocation == DropTargetLocation.Item || // Cannot drag recordings onto recordings
                sourceNode.Recording != null && sourceNode.DropDetails.Owner != this; // Do not allow recordings from other windows
        }

        #endregion

        private void OnFixtureRemoved(TestFixture closedfixture)
        {
            if (m_TestFixture == closedfixture)
                this.Close();
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

        protected override void ASSERT_TreeViewIsTheSameAsInRecordingManager()
        {
#if ENABLE_UI_TESTING
            try
            {
                var list = new List<HierarchyNode>();
                foreach (var n in m_Nodes[0].Children)
                    list.Add(n);
                foreach (var n in m_Nodes[1].Children)
                    list.Add(n);

                for (int i = 0; i < list.Count; i++)
                {
                    Logger.Instance.AssertIf(list[i].Recording != m_TestFixture.LoadedRecordings[i],
                        string.Format("Hierarchy recording missmatch: {0}:{1}", i, list[i].Value.ToString()));

                    // Will not work in nested scenarios
                    for (int j = 0; j < list[i].Recording.Commands.Count(); j++)
                    {
                        Logger.Instance.AssertIf(list[i].Children[j].Command != m_TestFixture.LoadedRecordings[i].Commands.GetChild(j).value,
                            string.Format("Hierarchy command missmatch: {0}:{1}, {2}:{3}",
                            i, list[i].Value.ToString(), j, list[i].Recording.Commands.GetChild(j).value.ToString()));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Exception in ASSERT_TreeViewIsTheSameAsInRecordingManager: " + e.Message);
            }
#endif
        }
    }
}
