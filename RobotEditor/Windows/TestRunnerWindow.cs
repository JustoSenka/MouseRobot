#define ENABLE_UI_TESTING

using BrightIdeasSoftware;
using Robot.Abstractions;
using Robot.Tests;
using RobotEditor.Hierarchy;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor
{
    public partial class TestRunnerWindow : DockContent, ITestRunnerWindow
    {
        private List<TestNode> m_Nodes = new List<TestNode>();

        private TestNode m_HighlightedNode;

        private readonly IMouseRobot MouseRobot;
        private readonly ITestRunnerManager TestRunnerManager;
        private readonly ITestStatusManager TestStatusManager;
        private readonly ITestRunner TestRunner;
        public TestRunnerWindow(IMouseRobot MouseRobot, ITestRunnerManager TestRunnerManager, ITestStatusManager TestStatusManager, ITestRunner TestRunner)
        {
            this.MouseRobot = MouseRobot;
            this.TestRunnerManager = TestRunnerManager;
            this.TestStatusManager = TestStatusManager;
            this.TestRunner = TestRunner;

            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;

            treeListView.Font = Fonts.Default;

            TestRunnerManager.TestFixtureAdded += OnTestFixtureAdded;
            TestRunnerManager.TestFixtureRemoved += OnTestFixtureRemoved;
            TestRunnerManager.TestFixtureModified += OnTestFixtureModified;

            TestStatusManager.TestStatusUpdated += UpdateTestStatusIconsAsync;

            TestRunner.TestRunEnd += OnTestRunEnd;
            TestRunner.FixtureIsBeingRun += OnFixtureIsBeingRun;
            TestRunner.TestIsBeingRun += OnTestIsBeingRun;

            MouseRobot.PlayingStateChanged += OnPlayingStateChanged;

            treeListView.HandleCreated += (sender, args) => UpdateHierarchyAsync();

            CreateColumns();
            UpdateHierarchyAsync();
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
                else if (node.Recording != null)
                    return node.Recording.Name;
                else
                    return node.ToString();
            };

            nameColumn.ImageGetter += delegate (object x)
            {
                var node = (TestNode)x;
                var fixture = node.TestFixture ?? node.Parent.TestFixture;

                if (node.TestFixture != null)
                {
                    return (int)TestStatusManager.GetFixtureStatus(fixture.ToLightTestFixture());
                }
                else
                {
                    var tuple = (fixture.Name, node.Recording.Name);
                    TestStatusManager.CurrentStatus.TryGetValue(tuple, out TestStatus status);

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
            if (node == m_HighlightedNode)
                e.SubItem.BackColor = SystemColors.Highlight;
        }

        private void UpdateHierarchyAsync()
        {
            treeListView.BeginInvokeIfCreated(new MethodInvoker(() =>
            {
                m_Nodes.Clear();

                foreach (var f in TestRunnerManager.TestFixtures)
                    m_Nodes.Add(new TestNode(f));

                RefreshTreeListView();
                treeListView.ExpandAll();

                ASSERT_TreeViewIsTheSameAsInRecordingManager();
            }));
        }

        private void RefreshTreeListView(bool performExpandAll = false)
        {
            treeListView.Roots = m_Nodes;

            if (treeListView.IsCreatedAndFuctional())
            {
                treeListView.Refresh();
                if (performExpandAll)
                    treeListView.ExpandAll();
            }
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
            treeListView.Expand(fixtureNode.Parent);

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        private void OnTestFixtureRemoved(TestFixture fixture, int position)
        {
            m_Nodes.RemoveAt(position);
            RefreshTreeListView();
            ASSERT_TreeViewIsTheSameAsInRecordingManager();
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

            ASSERT_TreeViewIsTheSameAsInRecordingManager();
        }

        private void OnTestRunEnd()
        {
            m_HighlightedNode = null;
            UpdateTestStatusIconsAsync();
            TestStatusManager.OutputTestRunStatusToFile();
        }

        private void UpdateTestStatusIconsAsync()
        {
            this.BeginInvokeIfCreated(new MethodInvoker(delegate
            {
                RefreshTreeListView();
            }));
        }

        private void OnFixtureIsBeingRun(LightTestFixture lightTestFixture)
        {
            var fixtureNode = m_Nodes.FirstOrDefault(node => node.TestFixture.Name == lightTestFixture.Name);
            if (fixtureNode != null)
            {
                m_HighlightedNode = fixtureNode;
                this.BeginInvokeIfCreated(new MethodInvoker(delegate
                {
                    RefreshTreeListView();
                }));
            }
        }

        private void OnTestIsBeingRun(LightTestFixture lightTestFixture, Recording recording)
        {
            var fixtureNode = m_Nodes.FirstOrDefault(node => node.TestFixture.Name == lightTestFixture.Name);
            if (fixtureNode != null && fixtureNode.TestFixture != null)
            {
                var recordingNode = fixtureNode.Children.FirstOrDefault(node => node.Recording?.Name == recording.Name);
                if (recordingNode != null)
                {
                    m_HighlightedNode = recordingNode;
                    this.BeginInvokeIfCreated(new MethodInvoker(delegate
                    {
                        RefreshTreeListView();
                    }));
                }
            }
        }

        private void treeListView_DoubleClick(object sender, EventArgs e)
        {
            var selected = treeListView.SelectedObject;
            if (selected == null)
                return;

            runSelectedToolStripMenuItem_Click(this, null);
        }

        private void OnPlayingStateChanged(bool isPlaying)
        {
            this.BeginInvokeIfCreated(new MethodInvoker(delegate
            {
                RunAllButton.Enabled = !MouseRobot.IsRecording && !MouseRobot.IsPlaying;
                RunDropdownButton.Enabled = !MouseRobot.IsRecording && !MouseRobot.IsPlaying;
                StopRunButton.Enabled = !MouseRobot.IsRecording && MouseRobot.IsPlaying;
            }));
        }

        #endregion

        #region Run Tests Toolstrip Buttons

        private void RunAllButton_Click(object sender, EventArgs e)
        {
            StartTestsWithSafeChecks();
        }

        private void runSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var names = treeListView.SelectedObjects.Cast<TestNode>().Select(node =>
                node.TestFixture != null ? node.TestFixture.Name : node.Parent.TestFixture.Name + "\\." + node.Recording.Name);

            var filter = BuildTestFilter(names);
            if (filter != "")
                StartTestsWithSafeChecks(filter);
        }

        private void runFailedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var names = TestStatusManager.CurrentStatus.Where(pair => pair.Value == TestStatus.Failed).
                Select(pair => pair.Key.Item1 + "\\." + pair.Key.Item2);

            var filter = BuildTestFilter(names);
            if (filter != "")
                StartTestsWithSafeChecks(filter);
        }

        private void runNotRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var names = TestStatusManager.CurrentStatus.Where(pair => pair.Value == TestStatus.None).
                Select(pair => pair.Key.Item1 + "\\." + pair.Key.Item2);

            var filter = BuildTestFilter(names);
            if (filter != "")
                StartTestsWithSafeChecks(filter);
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

        private string BuildTestFilter(IEnumerable<string> names)
        {
            var builder = new StringBuilder();

            foreach (var name in names)
                builder.Append("^" + name + "$|");

            var filter = builder.ToString();
            filter = filter.TrimEnd('|'); // removing last vertical bar symbol
            return filter;
        }

        #endregion

        private void ASSERT_TreeViewIsTheSameAsInRecordingManager()
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
