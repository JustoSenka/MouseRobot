// #define ENABLE_UI_TESTING

using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using RobotRuntime;
using BrightIdeasSoftware;
using System.Collections.Generic;
using System.Diagnostics;
using Robot.Abstractions;
using RobotEditor.Hierarchy;
using RobotRuntime.Tests;
using RobotRuntime.Abstractions;
using System.Text;

namespace RobotEditor
{
    public partial class TestRunnerWindow : DockContent, ITestRunnerWindow
    {
        private List<TestNode> m_Nodes = new List<TestNode>();

        // private TestNode m_HighlightedNode;

        private IMouseRobot MouseRobot;
        private ITestRunnerManager TestRunnerManager;
        private ITestRunner TestRunner;
        public TestRunnerWindow(IMouseRobot MouseRobot, ITestRunnerManager TestRunnerManager, ITestRunner TestRunner)
        {
            this.MouseRobot = MouseRobot;
            this.TestRunnerManager = TestRunnerManager;
            this.TestRunner = TestRunner;

            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;

            treeListView.Font = Fonts.Default;

            TestRunnerManager.TestFixtureAdded += OnTestFixtureAdded;
            TestRunnerManager.TestFixtureRemoved += OnTestFixtureRemoved;
            TestRunnerManager.TestFixtureModified += OnTestFixtureModified;

            MouseRobot.PlayingStateChanged += OnPlayingStateChanged;

            CreateColumns();

            UpdateHierarchy();
        }

        private void CreateColumns()
        {
            treeListView.CanExpandGetter = x => ((TestNode)x).Children.Count > 0;
            treeListView.ChildrenGetter = x => ((TestNode)x).Children;

            var nameColumn = new OLVColumn("Name", "Name");
            nameColumn.AspectGetter = x => x.ToString();

            nameColumn.ImageGetter += delegate (object x)
            {
                var imageListIndex = -1;
                var node = (TestNode)x;
                imageListIndex = node.TestFixture != null ? 0 : imageListIndex;
                imageListIndex = node.Script != null ? 1 : imageListIndex;
                return imageListIndex;
            };

            treeListView.FormatCell += UpdateFontsTreeListView;

            treeListView.UseCellFormatEvents = true;

            nameColumn.Width = treeListView.Width;
            treeListView.Columns.Add(nameColumn);
        }

        private void UpdateFontsTreeListView(object sender, FormatCellEventArgs e)
        {
            var node = e.Model as TestNode;
            if (node == null)
                return;
            /*
            if (node.Command != null)
            {
                if (node == m_HighlightedNode)
                    e.SubItem.BackColor = SystemColors.Highlight;
            }*/
        }

        private void UpdateHierarchy()
        {
            m_Nodes.Clear();

            foreach (var f in TestRunnerManager.TestFixtures)
                m_Nodes.Add(new TestNode(f));

            RefreshTreeListView();

            if (treeListView.Created)
                treeListView.ExpandAll();
        }

        private void RefreshTreeListView()
        {
            treeListView.Roots = m_Nodes;

            /* for (int i = 0; i < treeListView.Items.Count; ++i)
             {
                 treeListView.Items[i].ImageIndex = 0;
             }*/

            if (treeListView.Created)
                treeListView.Refresh();
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedNode = treeListView.SelectedObject as TestNode;
            if (selectedNode == null || selectedNode.TestFixture == null)
                return;

            Process.Start("explorer.exe", "/select, " + selectedNode.TestFixture.Path);
        }

        #region ToolStrip Buttons

        public ToolStrip ToolStrip { get { return toolStrip; } }

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

        #region TestRunnerManager Callbacks

        private void OnTestFixtureAdded(TestFixture fixture, int position)
        {
            var fixtureNode = new TestNode(fixture);
            m_Nodes.Insert(position, fixtureNode);
            RefreshTreeListView();
        }

        private void OnTestFixtureRemoved(TestFixture fixture, int position)
        {
            m_Nodes.RemoveAt(position);
            RefreshTreeListView();
        }

        private void OnTestFixtureModified(TestFixture fixture, int position)
        {
            var expandState = treeListView.IsExpanded(m_Nodes[position]);
            m_Nodes.RemoveAt(position);

            var fixtureNode = new TestNode(fixture);
            m_Nodes.Insert(position, fixtureNode);

            RefreshTreeListView();

            if (expandState)
                treeListView.Expand(fixtureNode);
        }

        #endregion


        private void treeListView_SelectionChanged(object sender, EventArgs e)
        {
        }

        private void OnPlayingStateChanged(bool isPlaying)
        {
            if (!Created || IsDisposed || Disposing)
                return;

            // Since Playing State can be changed from ScriptThread, we need to make sure we run this on UI thread
            this.BeginInvoke(new MethodInvoker(delegate
            {
                RunAllButton.Enabled = !MouseRobot.IsRecording && !MouseRobot.IsPlaying;
                RunDropdownButton.Enabled = !MouseRobot.IsRecording && !MouseRobot.IsPlaying;
                StopRunButton.Enabled = !MouseRobot.IsRecording && MouseRobot.IsPlaying;
            }));
        }

        #region Run Tests Toolstrip Buttons

        private void RunAllButton_Click(object sender, EventArgs e)
        {
            StartTestsWithSafeChecks();
        }

        private void runSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var builder = new StringBuilder();

            foreach (var item in treeListView.SelectedObjects)
                builder.Append("^" + item.ToString() + "$|");

            builder.Replace("*", ""); // Removing dirty signs if accidentally they appear

            var filter = builder.ToString();
            filter = filter.TrimEnd('|'); // removing last vertical bar symbol

            StartTestsWithSafeChecks(filter);
        }

        private void runFailedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO:
        }

        private void runNotRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO:
        }

        private void StopRunButton_Click(object sender, EventArgs e)
        {
            MouseRobot.IsPlaying = false;
        }

        private void StartTestsWithSafeChecks(string testFilter = "")
        {
            if (MouseRobot.IsRecording || MouseRobot.IsPlaying)
                return;

            MouseRobot.IsPlaying = true;

            if (testFilter == "")
                TestRunner.StartTests();
            else
                TestRunner.StartTests(testFilter);
        }

        #endregion

        private void ASSERT_TreeViewIsTheSameAsInScriptManager()
        {
#if ENABLE_UI_TESTING
            for (int i = 0; i < m_Nodes.Count; i++)
            {
                Debug.Assert(m_Nodes[i].Script == ScriptManager.LoadedScripts[i],
                    string.Format("Hierarchy script missmatch: {0}:{1}", i, m_Nodes[i].Value.ToString()));

                // Will not work in nested scenarios
                for (int j = 0; j < m_Nodes[i].Script.Commands.Count(); j++)
                {
                    Debug.Assert(m_Nodes[i].Children[j].Command == ScriptManager.LoadedScripts[i].Commands.GetChild(j).value,
                        string.Format("Hierarchy command missmatch: {0}:{1}, {2}:{3}",
                        i, m_Nodes[i].Value.ToString(), j, m_Nodes[i].Script.Commands.GetChild(j).value.ToString()));
                }
            }
#endif
        }
    }
}
