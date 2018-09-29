#define ENABLE_UI_TESTING

using BrightIdeasSoftware;
using Robot;
using Robot.Abstractions;
using Robot.Scripts;
using RobotEditor.Abstractions;
using RobotEditor.Hierarchy;
using RobotEditor.Utils;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Scripts;
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
        public event Action<BaseScriptManager, object> OnSelectionChanged;
        private List<HierarchyNode> m_Nodes = new List<HierarchyNode>();

        private HierarchyNode m_HighlightedNode;

        private IScriptManager ScriptManager;
        private ITestRunner TestRunner;
        private IAssetManager AssetManager;
        private ICommandFactory CommandFactory;
        private IHierarchyNodeStringConverter HierarchyNodeStringConverter;
        public HierarchyWindow(IScriptManager ScriptManager, ITestRunner TestRunner, IAssetManager AssetManager,
            IHierarchyNodeStringConverter HierarchyNodeStringConverter, ICommandFactory CommandFactory)
        {
            this.ScriptManager = ScriptManager;
            this.TestRunner = TestRunner;
            this.AssetManager = AssetManager;
            this.HierarchyNodeStringConverter = HierarchyNodeStringConverter;
            this.CommandFactory = CommandFactory;

            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;

            treeListView.Font = Fonts.Default;

            ScriptManager.CommandAddedToScript += OnCommandAddedToScript;
            ScriptManager.CommandRemovedFromScript += OnCommandRemovedFromScript;
            ScriptManager.CommandModifiedOnScript += OnCommandModifiedOnScript;
            ScriptManager.CommandInsertedInScript += OnCommandInsertedInScript;

            ScriptManager.ScriptAdded += OnScriptLoaded;
            ScriptManager.ScriptModified += OnScriptModified;
            ScriptManager.ScriptRemoved += OnScriptRemoved;
            ScriptManager.ScriptPositioningChanged += OnScriptPositioningChanged;

            TestRunner.TestRunEnd += OnScriptsFinishedRunning;
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
            if (CommandFactory == null || contextMenuStrip == null || treeListView == null || ScriptManager == null)
            {
                Logger.Log(LogType.Error, "HierarchyWindow.AddNewCommandsToCreateMenu() was called to early. Creating commands will not be possible. Please report a bug.",
                    "CommandFactory " + (CommandFactory == null) + ", contextMenuStrip " + (contextMenuStrip == null) +
                    ", treeListView " + (treeListView == null) + ", TestFixture " + (ScriptManager == null));
                return;
            }

            HierarchyUtils.OnNewUserCommandsAppeared(CommandFactory, contextMenuStrip, 8,
                treeListView, ScriptManager as BaseScriptManager);
        }

        private void UpdateFontsTreeListView(object sender, FormatCellEventArgs e)
        {
            var node = e.Model as HierarchyNode;
            if (node == null)
                return;

            if (node.Script != null)
            {
                if (node.Script == ScriptManager.ActiveScript && node.Script.IsDirty)
                    e.SubItem.Font = Fonts.ActiveAndDirtyScript;//.AddFont(Fonts.ActiveScript);
                else if (node.Script == ScriptManager.ActiveScript)
                    e.SubItem.Font = Fonts.ActiveScript;
                else if (node.Script.IsDirty)
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

        private void UpdateHierarchy(object sender, EventArgs args)
        {
            m_Nodes.Clear();

            foreach (var s in ScriptManager.LoadedScripts)
                m_Nodes.Add(new HierarchyNode(s));

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
                    ScriptManager.ActiveScript.AddCommand(command);
                };
                createMenuItem.DropDownItems.Add(item);
            }
        }

        #region ScriptManager Callbacks

        private void OnScriptLoaded(Script script)
        {
            var node = new HierarchyNode(script);
            m_Nodes.Add(node);

            RefreshTreeListViewAsync(() =>
            {
                treeListView.SelectedObject = node;
                treeListView.Expand(node);
            });

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        private void OnScriptModified(Script script)
        {
            var node = new HierarchyNode(script);
            var index = script.GetIndex(ScriptManager);
            m_Nodes[index] = node;
            RefreshTreeListViewAsync();

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        private void OnScriptRemoved(int index)
        {
            var oldSelectedObject = treeListView.SelectedObject;

            m_Nodes.RemoveAt(index);

            RefreshTreeListViewAsync(() =>
            {
                if (treeListView.SelectedObject != oldSelectedObject)
                    OnSelectionChanged?.Invoke((BaseScriptManager)ScriptManager, null);
            });

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        private void OnScriptPositioningChanged()
        {
            foreach (var script in ScriptManager.LoadedScripts)
            {
                var index = m_Nodes.FindIndex(n => n.Script == script);
                m_Nodes.MoveBefore(index, script.GetIndex(ScriptManager));
            }

            RefreshTreeListViewAsync();
            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        private void OnCommandAddedToScript(Script script, Command parentCommand, Command command)
        {
            var addedNode = HierarchyUtils.OnCommandAddedToScript(m_Nodes, script, parentCommand, command);
            var postRefreshAction = (addedNode.Parent.Children.Count == 1) ? () => treeListView.Expand(addedNode.Parent) : default(Action);

            RefreshTreeListViewAsync(postRefreshAction);
        }

        private void OnCommandRemovedFromScript(Script script, Command parentCommand, int commandIndex)
        {
            HierarchyUtils.OnCommandRemovedFromScript(m_Nodes, script, parentCommand, commandIndex);
            RefreshTreeListViewAsync();
        }

        private void OnCommandModifiedOnScript(Script script, Command oldCommand, Command newCommand)
        {
            HierarchyUtils.OnCommandModifiedOnScript(m_Nodes, script, oldCommand, newCommand);
            RefreshTreeListViewAsync();
        }

        // Will not work with multi dragging
        private void OnCommandInsertedInScript(Script script, Command parentCommand, Command command, int pos)
        {
            var node = HierarchyUtils.OnCommandInsertedInScript(m_Nodes, script, parentCommand, command, pos);
            RefreshTreeListViewAsync(() => treeListView.SelectedObject = node);
        }

        #endregion

        #region Context Menu Items
        private void setActiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedNode = treeListView.SelectedObject as HierarchyNode;
            if (selectedNode == null || selectedNode.Script == null)
                return;

            ScriptManager.ActiveScript = selectedNode.Script;
            RefreshTreeListViewAsync();
        }

        public void newScriptToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ScriptManager.NewScript();
            RefreshTreeListViewAsync();

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
                ScriptManager.NewScript(selectedNode.Script);
                ScriptManager.MoveScriptAfter(ScriptManager.LoadedScripts.Count - 1, selectedNode.Script.GetIndex(ScriptManager));
            }
            else if (selectedNode.Command != null)
            {
                var script = selectedNode.TopLevelNode.Script;
                var node = script.Commands.GetNodeFromValue(selectedNode.Command);
                var clone = (TreeNode<Command>)node.Clone();
                clone.CastAndRemoveNullsTree<IHaveGuid>().RegenerateGuids();

                script.AddCommandNode(clone, node.parent.value);
                script.MoveCommandAfter(clone.value, selectedNode.Command);
            }

            RefreshTreeListViewAsync();
            treeListView.Focus();

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        public void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var selectedNode = treeListView.SelectedObject as HierarchyNode;
            if (selectedNode == null)
                return;

            if (selectedNode.Script != null)
                ScriptManager.RemoveScript(selectedNode.Script);
            else if (selectedNode.Command != null)
                ScriptManager.GetScriptFromCommand(selectedNode.Command).RemoveCommand(selectedNode.Command);

            RefreshTreeListViewAsync();

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }
        #endregion

        #region Menu Items (save scripts from MainForm)
        public void SaveAllScripts()
        {
            foreach (var script in ScriptManager)
            {
                if (!script.IsDirty)
                    continue;

                if (script.Path != "")
                    ScriptManager.SaveScript(script, script.Path);
                else
                    SaveSelectedScriptWithDialog(script, updateUI: false);
            }

            RefreshTreeListViewAsync();
        }

        public void SaveSelectedScriptWithDialog(Script script, bool updateUI = true)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.InitialDirectory = Environment.CurrentDirectory + "\\" + Paths.ScriptFolder;
            saveDialog.Filter = string.Format("Mouse Robot File (*.{0})|*.{0}", FileExtensions.Script);
            saveDialog.Title = "Select a path for script to save.";
            saveDialog.FileName = script.Name + FileExtensions.ScriptD;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                ScriptManager.SaveScript(script, saveDialog.FileName);
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
                targetNode.Script == null && sourceNode.Command == null ||
                targetNode.Script != null && sourceNode.Script != null && e.DropTargetLocation == DropTargetLocation.Item)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.DropSink.CanDropOnItem = targetNode.Script != null || targetNode.Command.CanBeNested;

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
                    ScriptManager.MoveScriptBefore(sourceNode.Script.GetIndex(ScriptManager), targetNode.Script.GetIndex(ScriptManager));
                if (e.DropTargetLocation == DropTargetLocation.BelowItem)
                    ScriptManager.MoveScriptAfter(sourceNode.Script.GetIndex(ScriptManager), targetNode.Script.GetIndex(ScriptManager));
            }

            if (targetNode.Command != null && sourceNode.Command != null)
            {
                var targetScript = ScriptManager.GetScriptFromCommand(targetNode.Command);
                var sourceScript = ScriptManager.GetScriptFromCommand(sourceNode.Command);

                if (e.DropTargetLocation == DropTargetLocation.AboveItem)
                    ScriptManager.MoveCommandBefore(sourceNode.Command, targetNode.Command, sourceScript.GetIndex(ScriptManager), targetScript.GetIndex(ScriptManager));
                if (e.DropTargetLocation == DropTargetLocation.BelowItem)
                    ScriptManager.MoveCommandAfter(sourceNode.Command, targetNode.Command, sourceScript.GetIndex(ScriptManager), targetScript.GetIndex(ScriptManager));

                if (e.DropTargetLocation == DropTargetLocation.Item && targetNode.Command.CanBeNested)
                {
                    var node = sourceScript.Commands.GetNodeFromValue(sourceNode.Command);
                    sourceScript.RemoveCommand(sourceNode.Command);
                    targetScript.AddCommandNode(node, targetNode.Command);
                }
            }

            if (targetNode.Script != null && sourceNode.Command != null)
            {
                var sourceScript = ScriptManager.GetScriptFromCommand(sourceNode.Command);

                var node = sourceScript.Commands.GetNodeFromValue(sourceNode.Command);
                sourceScript.RemoveCommand(sourceNode.Command);
                targetNode.Script.AddCommandNode(node);
            }
        }

        #endregion

        #region ScriptRunner Callbacks

        private void OnCommandRunning(Command command)
        {
            var script = ScriptManager.GetScriptFromCommand(command);
            if (script == null)
                return;

            var scriptNode = m_Nodes.FirstOrDefault(node => node.Script == script);
            if (scriptNode == null)
                return;

            var commandNode = scriptNode.GetNodeFromValue(command);
            m_HighlightedNode = commandNode;

            treeListView.BeginInvokeIfCreated(new MethodInvoker(() => treeListView.Refresh()));
        }

        private void OnScriptsFinishedRunning()
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
                OnSelectionChanged?.Invoke((BaseScriptManager)ScriptManager, null);
            }
            else
            {
                OnSelectionChanged?.Invoke((BaseScriptManager)ScriptManager, node.Value);
            }
        }

        private void TestFixtureWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            TestRunner.TestRunEnd -= OnScriptsFinishedRunning;
            TestRunner.TestData.CommandRunningCallback -= OnCommandRunning;
            CommandFactory.NewUserCommands -= AddNewCommandsToCreateMenu;
            treeListView.HandleCreated -= AddNewCommandsToCreateMenu;
            contextMenuStrip.HandleCreated -= AddNewCommandsToCreateMenu;
            treeListView.FormatCell -= UpdateFontsTreeListView;
            treeListView.HandleCreated -= UpdateHierarchy;
        }

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
