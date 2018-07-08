#define ENABLE_UI_TESTING

using System;
using RobotEditor.Abstractions;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using RobotRuntime;
using BrightIdeasSoftware;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Drawing;
using Robot.Abstractions;
using RobotRuntime.Abstractions;
using RobotEditor.Hierarchy;
using RobotRuntime.Scripts;
using RobotRuntime.Tests;
using RobotRuntime.Utils;
using RobotEditor.Utils;
using Robot.Scripts;

namespace RobotEditor
{
    public partial class TestFixtureWindow : DockContent
    {
        public event Action<BaseScriptManager, object> OnSelectionChanged;
        private List<HierarchyNode> m_Nodes = new List<HierarchyNode>();

        private HierarchyNode m_HighlightedNode;

        public TestFixture TestFixture;
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

            TestRunner.Finished += OnScriptsFinishedRunning;
            TestRunner.RunningCommandCallback += OnCommandRunning;

            CommandFactory.NewUserCommands += AddNewCommandsToCreateMenu;
            AddNewCommandsToCreateMenu();

            treeListView.FormatCell += UpdateFontsTreeListView;
            HierarchyUtils.CreateColumns(treeListView, HierarchyNodeStringConverter);

            UpdateHierarchy();
        }

        private void AddNewCommandsToCreateMenu()
        {
            HierarchyUtils.OnNewUserCommandsAppeared(CommandFactory, contextMenuStrip, 5,
                treeListView, TestFixture);
        }

        public void DisplayTestFixture(TestFixture fixture)
        {
            if (fixture == null || TestFixture == fixture)
                return;

            if (TestFixture != null)
                UnsubscribeAllEvents(TestFixture);

            TestFixture = fixture;
            SubscribeALlEvents(TestFixture);
            UpdateHierarchy();
        }

        private void SubscribeALlEvents(TestFixture fixture)
        {
            fixture.CommandAddedToScript += OnCommandAddedToScript;
            fixture.CommandRemovedFromScript += OnCommandRemovedFromScript;
            fixture.CommandModifiedOnScript += OnCommandModifiedOnScript;
            fixture.CommandInsertedInScript += OnCommandInsertedInScript;

            fixture.ScriptAdded += OnScriptLoaded;
            fixture.ScriptModified += OnScriptModified;
            fixture.ScriptRemoved += OnScriptRemoved;
            fixture.ScriptPositioningChanged += OnScriptPositioningChanged;
        }

        private void UnsubscribeAllEvents(TestFixture fixture)
        {
            fixture.CommandAddedToScript -= OnCommandAddedToScript;
            fixture.CommandRemovedFromScript -= OnCommandRemovedFromScript;
            fixture.CommandModifiedOnScript -= OnCommandModifiedOnScript;
            fixture.CommandInsertedInScript -= OnCommandInsertedInScript;

            fixture.ScriptAdded -= OnScriptLoaded;
            fixture.ScriptModified -= OnScriptModified;
            fixture.ScriptRemoved -= OnScriptRemoved;
            fixture.ScriptPositioningChanged -= OnScriptPositioningChanged;
        }

        private void UpdateFontsTreeListView(object sender, FormatCellEventArgs e)
        {
            if (!(e.Model is HierarchyNode node))
                return;

            if (node.Script != null)
            {
                if (node.Script.IsDirty)
                    e.SubItem.Font = Fonts.DirtyScript;//.AddFont(Fonts.DirtyScript);
                else
                    e.SubItem.Font = Fonts.Default;
            }

            if (node.Command != null)
            {
                if (node == m_HighlightedNode)
                    e.SubItem.BackColor = SystemColors.Highlight;
            }
        }

        public void UpdateHierarchy()
        {
            m_Nodes.Clear();

            if (TestFixture == null)
                return;

            m_HooksNode = new HierarchyNode("Special Scripts");
            foreach (var s in TestFixture.Hooks)
                m_HooksNode.AddHierarchyNode(new HierarchyNode(s));

            m_TestsNode = new HierarchyNode("Tests");
            foreach (var s in TestFixture.Tests)
                m_TestsNode.AddHierarchyNode(new HierarchyNode(s));

            m_Nodes.Add(m_HooksNode);
            m_Nodes.Add(m_TestsNode);

            RefreshTreeListView();
            treeListView.ExpandAll();
        }

        private void RefreshTreeListView()
        {
            this.Text = TestFixture.ToString();
            treeListView.Roots = m_Nodes;

            for (int i = 0; i < treeListView.Items.Count; ++i)
            {
                treeListView.Items[i].ImageIndex = 0;
            }
            treeListView.Refresh();
        }

        #region ScriptManager Callbacks

        private void OnScriptLoaded(Script script)
        {
            var node = new HierarchyNode(script);
            m_TestsNode.AddHierarchyNode(node);
            RefreshTreeListView();

            treeListView.SelectedObject = node;
            treeListView.Expand(node);

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        private void OnScriptModified(Script script)
        {
            var node = new HierarchyNode(script);
            m_Nodes.ReplaceNodeWithNewOne(node);

            RefreshTreeListView();

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        private void OnScriptRemoved(int index)
        {
            var oldSelectedObject = treeListView.SelectedObject;

            m_Nodes.RemoveAtIndexRemoving4(index);
            RefreshTreeListView();

            if (treeListView.SelectedObject != oldSelectedObject)
                OnSelectionChanged?.Invoke(TestFixture, null);

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        private void OnScriptPositioningChanged()
        {
            foreach (var script in TestFixture.Tests)
            {
                var index = m_TestsNode.Children.FindIndex(n => n.Script == script);
                var indexInBackend = TestFixture.GetScriptIndex(script) - 4; // -4 since 4 scripts are reserved as hooks
                m_TestsNode.Children.MoveBefore(index, indexInBackend);
            }

            // Hooks do not allow drag and drop, but keeping here in case of position changing from script
            foreach (var script in TestFixture.Hooks)
            {
                var index = m_HooksNode.Children.FindIndex(n => n.Script == script);
                m_HooksNode.Children.MoveBefore(index, TestFixture.GetScriptIndex(script));
            }

            RefreshTreeListView();
            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        private void OnCommandAddedToScript(Script script, Command parentCommand, Command command)
        {
            HierarchyUtils.OnCommandAddedToScript(m_Nodes, script, parentCommand, command);
            RefreshTreeListView();
        }

        private void OnCommandRemovedFromScript(Script script, Command parentCommand, int commandIndex)
        {
            HierarchyUtils.OnCommandRemovedFromScript(m_Nodes, script, parentCommand, commandIndex);
            RefreshTreeListView();
        }

        private void OnCommandModifiedOnScript(Script script, Command oldCommand, Command newCommand)
        {
            HierarchyUtils.OnCommandModifiedOnScript(m_Nodes, script, oldCommand, newCommand);
            RefreshTreeListView();
        }

        // Will not work with multi dragging
        private void OnCommandInsertedInScript(Script script, Command parentCommand, Command command, int pos)
        {
            var node = HierarchyUtils.OnCommandInsertedInScript(m_Nodes, script, parentCommand, command, pos);
            RefreshTreeListView();
            treeListView.SelectedObject = node;
        }

        #endregion

        #region Context Menu Items

        public void newScriptToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TestFixture.NewScript();
            RefreshTreeListView();

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        public void duplicateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!(treeListView.SelectedObject is HierarchyNode selectedNode))
                return;

            if (selectedNode.Script != null && IsItASpecialScript(selectedNode.Script))
            {
                TestFixture.NewScript(selectedNode.Script);
                TestFixture.MoveScriptAfter(TestFixture.LoadedScripts.Count - 1, TestFixture.GetScriptIndex(selectedNode.Script));
            }
            else if (selectedNode.Command != null)
            {
                var script = selectedNode.TopLevelScriptNode.Script;
                var node = script.Commands.GetNodeFromValue(selectedNode.Command);
                var clone = (TreeNode<Command>)node.Clone();

                script.AddCommandNode(clone, node.parent.value);
                script.MoveCommandAfter(clone.value, selectedNode.Command);
                //selectedNode.TopLevelScriptNode.Script.InsertCommandAfter(clone, selectedNode.Command);
            }

            RefreshTreeListView();
            treeListView.Focus();

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        public void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var selectedNode = treeListView.SelectedObject as HierarchyNode;
            if (selectedNode == null)
                return;

            if (selectedNode.Script != null && IsItASpecialScript(selectedNode.Script))
                TestFixture.RemoveScript(selectedNode.Script);
            else if (selectedNode.Command != null)
                TestFixture.GetScriptFromCommand(selectedNode.Command).RemoveCommand(selectedNode.Command);

            RefreshTreeListView();

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }
        #endregion

        #region Menu Items (save scripts from MainForm)
        public void SaveTestFixture()
        {
            if (!TestFixture.IsDirty)
                return;

            if (TestFixture.Path != "")
                TestFixtureManager.SaveTestFixture(TestFixture, TestFixture.Path);
            else
                SaveFixtureWithDialog(false);

            RefreshTreeListView();
        }

        public void SaveFixtureWithDialog(bool updateUI = true)
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                InitialDirectory = Environment.CurrentDirectory + "\\" + Paths.TestsFolder,
                Filter = string.Format("Test Fixture File (*.{0})|*.{0}", FileExtensions.Test),
                Title = "Select a path for script to save.",
                FileName = TestFixture.Name + FileExtensions.TestD
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                TestFixtureManager.SaveTestFixture(TestFixture, saveDialog.FileName);
                if (updateUI)
                    RefreshTreeListView();
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
                sourceNode.Script == null && sourceNode.Command == null || // Source node is empty string value
                targetNode.Script == null && targetNode.Command == null || // Target node is empty string value
                targetNode.Script == null && sourceNode.Command == null || // Cannot drag scripts onto commands
                m_HooksNode.Children.Contains(sourceNode) || // Hooks scripts are special and should not be moved at all
                m_HooksNode.Children.Contains(targetNode) && sourceNode.Script != null || // Cannot drag any script onto or inbetween hooks scripts
                targetNode.Script != null && sourceNode.Script != null && e.DropTargetLocation == DropTargetLocation.Item) // Cannot drag scripts onto scripts
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.DropSink.CanDropOnItem = sourceNode.Script == null && // We don't want scripts to be dropped on any item
                (targetNode.Script != null || targetNode.Command.CanBeNested); // Everything can be dropped on script and commands with nested tag can also be dropped onto

            if (targetNode.Script != null && sourceNode.Command != null)
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

            if (targetNode.Script != null && sourceNode.Script != null)
            {
                if (e.DropTargetLocation == DropTargetLocation.AboveItem)
                    TestFixture.MoveScriptBefore(TestFixture.GetScriptIndex(sourceNode.Script), TestFixture.GetScriptIndex(targetNode.Script));
                if (e.DropTargetLocation == DropTargetLocation.BelowItem)
                    TestFixture.MoveScriptAfter(TestFixture.GetScriptIndex(sourceNode.Script), TestFixture.GetScriptIndex(targetNode.Script));
            }

            if (targetNode.Command != null && sourceNode.Command != null)
            {
                var targetScript = TestFixture.GetScriptFromCommand(targetNode.Command);
                var sourceScript = TestFixture.GetScriptFromCommand(sourceNode.Command);

                if (e.DropTargetLocation == DropTargetLocation.AboveItem)
                    TestFixture.MoveCommandBefore(sourceNode.Command, targetNode.Command, TestFixture.GetScriptIndex(sourceScript), TestFixture.GetScriptIndex(targetScript));
                if (e.DropTargetLocation == DropTargetLocation.BelowItem)
                    TestFixture.MoveCommandAfter(sourceNode.Command, targetNode.Command, TestFixture.GetScriptIndex(sourceScript), TestFixture.GetScriptIndex(targetScript));

                if (e.DropTargetLocation == DropTargetLocation.Item && targetNode.Command.CanBeNested)
                {
                    var node = sourceScript.Commands.GetNodeFromValue(sourceNode.Command);
                    sourceScript.RemoveCommand(sourceNode.Command);
                    targetScript.AddCommandNode(node, targetNode.Command);
                }
            }

            if (targetNode.Script != null && sourceNode.Command != null)
            {
                var sourceScript = TestFixture.GetScriptFromCommand(sourceNode.Command);

                var node = sourceScript.Commands.GetNodeFromValue(sourceNode.Command);
                sourceScript.RemoveCommand(sourceNode.Command);
                targetNode.Script.AddCommandNode(node);
            }
        }

        #endregion

        #region ScriptRunner Callbacks

        // TODO: Not tested if works.
        // TODO: Also mark parent commands/scripts/tests
        private void OnCommandRunning(Command command)
        {
            var script = TestFixture.GetScriptFromCommand(command);
            if (script == null)
                return;

            var scriptNode = m_Nodes.FindRecursively(script);
            if (scriptNode == null)
                return;

            var commandNode = scriptNode.GetNodeFromValue(command);
            m_HighlightedNode = commandNode;

            if (treeListView.Created)
                treeListView.Invoke(new Action(() => treeListView.Refresh()));
        }

        private void OnScriptsFinishedRunning()
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
                OnSelectionChanged?.Invoke(TestFixture, null);
            }
            else
            {
                OnSelectionChanged?.Invoke(TestFixture, node.Value);
            }
        }

        private void TestFixtureWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            TestFixtureManager.Remove(TestFixture);

            TestRunner.Finished -= OnScriptsFinishedRunning;
            TestRunner.RunningCommandCallback -= OnCommandRunning;
            CommandFactory.NewUserCommands -= AddNewCommandsToCreateMenu;
        }

        private bool IsItASpecialScript(Script script)
        {
            if (script == null)
                return false;

            return TestFixture.GetScriptIndex(script) >= 4;
        }

        private void ASSERT_TreeViewIsTheSameAsInScriptManager()
        {
#if ENABLE_UI_TESTING

            var list = new List<HierarchyNode>();
            foreach (var n in m_Nodes[0].Children)
                list.Add(n);
            foreach (var n in m_Nodes[1].Children)
                list.Add(n);

            for (int i = 0; i < list.Count; i++)
            {
                Debug.Assert(list[i].Script == TestFixture.LoadedScripts[i],
                    string.Format("Hierarchy script missmatch: {0}:{1}", i, list[i].Value.ToString()));

                // Will not work in nested scenarios
                for (int j = 0; j < list[i].Script.Commands.Count(); j++)
                {
                    Debug.Assert(list[i].Children[j].Command == TestFixture.LoadedScripts[i].Commands.GetChild(j).value,
                        string.Format("Hierarchy command missmatch: {0}:{1}, {2}:{3}",
                        i, list[i].Value.ToString(), j, list[i].Script.Commands.GetChild(j).value.ToString()));
                }
            }
#endif
        }
    }
}
