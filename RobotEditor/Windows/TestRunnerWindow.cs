#define ENABLE_UI_TESTING

using BrightIdeasSoftware;
using Robot.Abstractions;
using Robot.Tests;
using RobotEditor.Hierarchy;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Scripts;
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

            TestRunnerManager.TestStatusUpdated += UpdateTestStatusIconsAsync;

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
                    return (int)TestRunnerManager.GetFixtureStatus(fixture);
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

                ASSERT_TreeViewIsTheSameAsInScriptManager();
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

        private void OnTestRunEnd()
        {
            m_HighlightedNode = null;
            UpdateTestStatusIconsAsync();
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

        private void OnTestIsBeingRun(LightTestFixture lightTestFixture, Script script)
        {
            var fixtureNode = m_Nodes.FirstOrDefault(node => node.TestFixture.Name == lightTestFixture.Name);
            if (fixtureNode != null && fixtureNode.TestFixture != null)
            {
                var scriptNode = fixtureNode.Children.FirstOrDefault(node => node.Script?.Name == script.Name);
                if (scriptNode != null)
                {
                    m_HighlightedNode = scriptNode;
                    this.BeginInvokeIfCreated(new MethodInvoker(delegate
                    {
                        RefreshTreeListView();
                    }));
                }
            }
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
            this.BeginInvokeIfCreated(new MethodInvoker(delegate
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
            var names = treeListView.SelectedObjects.Cast<TestNode>().Select(node =>
                node.TestFixture != null ? node.TestFixture.Name : node.Parent.TestFixture.Name + "." + node.Script.Name);

            var filter = BuildTestFilter(names);
            if (filter != "")
                StartTestsWithSafeChecks(filter);
        }

        private void runFailedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var names = TestRunnerManager.TestStatusDictionary.Where(pair => pair.Value == TestStatus.Failed).
                Select(pair => pair.Key.Item1 + "." + pair.Key.Item2);

            var filter = BuildTestFilter(names);
            if (filter != "")
                StartTestsWithSafeChecks(filter);
        }

        private void runNotRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var names = TestRunnerManager.TestStatusDictionary.Where(pair => pair.Value == TestStatus.None).
                Select(pair => pair.Key.Item1 + "." + pair.Key.Item2);

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
