#define ENABLE_UI_TESTING

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
using Robot.Tests;

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

            TestRunnerManager.TestStatusUpdated += UpdateTestStatusIcons;
            TestRunner.TestRunEnd += UpdateTestStatusIcons;

            MouseRobot.PlayingStateChanged += OnPlayingStateChanged;
            
            CreateColumns();

            UpdateHierarchy();
        }

        private void CreateColumns()
        {
            treeListView.CanExpandGetter = x => ((TestNode)x).Children.Count > 0;
            treeListView.ChildrenGetter = x => ((TestNode)x).Children;

            var nameColumn = new OLVColumn("Name", "Name");
            nameColumn.AspectGetter = x =>
            {
                var node = ((TestNode)x);
                if (node.TestFixture != null)
                    return node.TestFixture.Name + " (" + node.TestFixture.Tests.Count + ")";
                else if (node.Script != null)
                    return node.Script.Name;
                else
                    return node.ToString();
            };

            nameColumn.ImageGetter += delegate (object x)
            {
                var node = (TestNode)x;
                var fixture = node.TestFixture ?? node.Parent.TestFixture;

                if (node.TestFixture != null)
                {
                    return (int) TestRunnerManager.GetFixtureStatus(fixture);
                }
                else
                {
                    var tuple = new Tuple<string, string>(fixture.Name, node.Script.Name);

                    TestRunnerManager.TestStatusDictionary.TryGetValue(tuple, out TestStatus status);

                    return (int)status;
                }
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

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        private void RefreshTreeListView()
        {
            treeListView.Roots = m_Nodes;

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
            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        private void OnTestFixtureRemoved(TestFixture fixture, int position)
        {
            m_Nodes.RemoveAt(position);
            RefreshTreeListView();
            ASSERT_TreeViewIsTheSameAsInScriptManager();
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

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        private void UpdateTestStatusIcons()
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                RefreshTreeListView();
            }));
        }

        #endregion

        private void treeListView_DoubleClick(object sender, EventArgs e)
        {
            var selected = treeListView.SelectedObject;
            if (selected == null)
                return;

            runSelectedToolStripMenuItem_Click(this, null);
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
                Debug.Assert(m_Nodes[i].TestFixture == TestRunnerManager.TestFixtures[i],
                    string.Format("Fixture missmatch: {0}:{1}", i, m_Nodes[i].Value.ToString()));

                for (int j = 0; j < m_Nodes[i].TestFixture.Tests.Count; j++)
                {
                    Debug.Assert(m_Nodes[i].TestFixture.Tests[j] == TestRunnerManager.TestFixtures[i].Tests[j],
                        string.Format("Fixture test missmatch: {0}:{1}, {2}:{3}",
                        i, m_Nodes[i].Value.ToString(), j, m_Nodes[i].TestFixture.Tests[j].ToString()));
                }
            }
#endif
        }
    }
}
