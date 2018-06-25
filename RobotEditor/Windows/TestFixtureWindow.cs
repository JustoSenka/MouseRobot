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
using Unity;
using Robot.Tests;
using RobotRuntime.Utils;

namespace RobotEditor
{
    public partial class TestFixtureWindow : DockContent
    {
        public event Action<Command> OnCommandSelected;
        private List<HierarchyNode> m_Nodes = new List<HierarchyNode>();

        private HierarchyNode m_HighlightedNode;

        public TestFixture TestFixture;
        private HierarchyNode m_HooksNode;
        private HierarchyNode m_TestsNode;

        private ITestRunner TestRunner;
        private IAssetManager AssetManager;
        private ITestFixtureManager TestFixtureManager;
        private IHierarchyNodeStringConverter HierarchyNodeStringConverter;
        private ICommandFactory CommandFactory;
        public TestFixtureWindow(ITestRunner TestRunner, IAssetManager AssetManager, ITestFixtureManager TestFixtureManager,
            IHierarchyNodeStringConverter HierarchyNodeStringConverter, ICommandFactory CommandFactory)
        {
            this.TestRunner = TestRunner;
            this.AssetManager = AssetManager;
            this.TestFixtureManager = TestFixtureManager;
            this.HierarchyNodeStringConverter = HierarchyNodeStringConverter;
            this.CommandFactory = CommandFactory;

            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;

            treeListView.Font = Fonts.Default;

            TestRunner.Finished += OnScriptsFinishedRunning;
            TestRunner.RunningCommandCallback += OnCommandRunning;

            CommandFactory.NewUserCommands += OnNewUserCommandsAppeared;
            OnNewUserCommandsAppeared();

            TestFixtureManager.FixtureAdded += DisplayTestFixture;

            CreateColumns();
            UpdateHierarchy();
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

        private void CreateColumns()
        {
            treeListView.CanExpandGetter = x => (x as HierarchyNode).Children.Count > 0;
            treeListView.ChildrenGetter = x => (x as HierarchyNode).Children;

            var nameColumn = new OLVColumn("Name", "Name");
            nameColumn.AspectGetter = x => HierarchyNodeStringConverter.ToString(x as HierarchyNode);

            nameColumn.ImageGetter += delegate (object x)
            {
                var imageListIndex = -1;
                var node = (HierarchyNode)x;
                imageListIndex = node.Script != null ? 0 : imageListIndex;
                imageListIndex = node.Command != null ? 1 : imageListIndex;
                return imageListIndex;
            };

            treeListView.UseCellFormatEvents = true;
            treeListView.FormatCell += UpdateFontsTreeListView;

            treeListView.IsSimpleDragSource = true;
            treeListView.IsSimpleDropSink = true;

            nameColumn.Width = treeListView.Width;
            treeListView.Columns.Add(nameColumn);
        }

        private void UpdateFontsTreeListView(object sender, FormatCellEventArgs e)
        {
            var node = e.Model as HierarchyNode;
            if (node == null)
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

        // TODO: if text fixture changes in the middle of runtime, will this still work due to referencing old test fixture in lamda?
        private void OnNewUserCommandsAppeared()
        {
            var createMenuItem = (ToolStripMenuItem)contextMenuStrip.Items[7];

            createMenuItem.DropDownItems.Clear();
            foreach (var name in CommandFactory.CommandNames)
            {
                var item = new ToolStripMenuItem(name);
                item.Click += (sender, events) =>
                {
                    var command = CommandFactory.Create(name);
                    TestFixture.Setup.AddCommand(command);
                };
                createMenuItem.DropDownItems.Add(item);
            }
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
                OnCommandSelected?.Invoke(null);

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
            var parentNode = script.Commands.GetNodeFromValue(command).parent;
            System.Diagnostics.Debug.Assert(parentNode.value == parentCommand, "parentCommand and parentNode missmatched");

            var scriptNode = m_Nodes.FindRecursively(script);

            var parentHierarchyNode = parentCommand == null ? scriptNode : scriptNode.GetNodeFromValue(parentNode.value);
            AddCommandToParentRecursive(script, command, parentHierarchyNode);

            if (scriptNode.Children.Count == 1)
                treeListView.Expand(scriptNode);

            RefreshTreeListView();
        }

        private static HierarchyNode AddCommandToParentRecursive(Script script, Command command, HierarchyNode parentHierarchyNode, int pos = -1)
        {
            var nodeToAdd = new HierarchyNode(command, parentHierarchyNode);

            if (pos == -1)
                parentHierarchyNode.Children.Add(nodeToAdd);
            else
                parentHierarchyNode.Children.Insert(pos, nodeToAdd);

            var commandNode = script.Commands.GetNodeFromValue(command);
            foreach (var childNode in commandNode)
                AddCommandToParentRecursive(script, childNode.value, nodeToAdd);

            return nodeToAdd;
        }

        private void OnCommandRemovedFromScript(Script script, Command parentCommand, int commandIndex)
        {
            var scriptNode = m_Nodes.FindRecursively(script);
            var parentNode = parentCommand == null ? scriptNode : scriptNode.GetNodeFromValue(parentCommand);

            parentNode.Children.RemoveAt(commandIndex);
            RefreshTreeListView();
        }

        private void OnCommandModifiedOnScript(Script script, Command oldCommand, Command newCommand)
        {
            var scriptNode = m_Nodes.FindRecursively(script);
            var commandNode = scriptNode.GetNodeFromValue(oldCommand);

            commandNode.Update(newCommand);
            RefreshTreeListView();
        }

        // Will not work with multi dragging
        private void OnCommandInsertedInScript(Script script, Command parentCommand, Command command, int pos)
        {
            var scriptNode = m_Nodes.FindRecursively(script);
            var parentNode = parentCommand == null ? scriptNode : scriptNode.GetNodeFromValue(parentCommand);

            var node = AddCommandToParentRecursive(script, command, parentNode, pos);

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

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedNode = treeListView.SelectedObject as HierarchyNode;
            if (selectedNode == null || selectedNode.Script == null)
                return;

            Process.Start("explorer.exe", "/select, " + selectedNode.Script.Path);
        }

        public void duplicateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var selectedNode = treeListView.SelectedObject as HierarchyNode;
            if (selectedNode == null)
                return;

            if (selectedNode.Script != null)
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

            if (selectedNode.Script != null)
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
                SaveSelectedScriptWithDialog(TestFixture, updateUI: false);

            RefreshTreeListView();
        }

        public void SaveSelectedScriptWithDialog(TestFixture fixture, bool updateUI = true)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.InitialDirectory = Environment.CurrentDirectory + "\\" + Paths.TestsFolder;
            saveDialog.Filter = string.Format("Test Fixture File (*.{0})|*.{0}", FileExtensions.Test);
            saveDialog.Title = "Select a path for script to save.";
            saveDialog.FileName = fixture.Name + FileExtensions.TestD;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                TestFixtureManager.SaveTestFixture(fixture, saveDialog.FileName);
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
            var node = treeListView.SelectedObject as HierarchyNode;

            if (node == null)
            {
                OnCommandSelected?.Invoke(null);
            }
            else if (node.Command != null)
            {
                OnCommandSelected?.Invoke(node.Command);
            }
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
