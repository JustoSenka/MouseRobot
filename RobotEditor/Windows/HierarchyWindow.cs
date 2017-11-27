#define ENABLE_UI_TESTING

using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using RobotRuntime;
using Robot;
using RobotEditor.Scripts;
using BrightIdeasSoftware;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Robot.Scripts;

namespace RobotEditor
{
    public partial class HierarchyWindow : DockContent
    {
        public event Action<Command> OnCommandSelected;
        private List<HierarchyNode> m_Nodes = new List<HierarchyNode>();

        public HierarchyWindow()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
            //treeView.NodeMouseClick += (sender, args) => treeView.SelectedNode = args.Node;

            treeListView.Font = Fonts.Default;

            ScriptManager.Instance.CommandAddedToScript += OnCommandAddedToScript;
            ScriptManager.Instance.CommandRemovedFromScript += OnCommandRemovedFromScript;
            ScriptManager.Instance.CommandModifiedOnScript += OnCommandModifiedOnScript;
            ScriptManager.Instance.CommandInsertedInScript += OnCommandInsertedInScript;

            ScriptManager.Instance.ScriptLoaded += OnScriptLoaded;
            ScriptManager.Instance.ScriptModified += OnScriptModified;
            ScriptManager.Instance.ScriptRemoved += OnScriptRemoved;
            ScriptManager.Instance.ScriptPositioningChanged += OnScriptPositioningChanged;

            CreateColumns();
            UpdateHierarchy();
        }

        private void CreateColumns()
        {
            treeListView.CanExpandGetter = x => (x as HierarchyNode).Children.Count > 0;
            treeListView.ChildrenGetter = x => (x as HierarchyNode).Children;

            var nameColumn = new OLVColumn("Name", "Name");
            nameColumn.AspectGetter = x => (x as HierarchyNode).Value.ToString();

            nameColumn.ImageGetter += delegate (object x)
            {
                var imageListIndex = (x as HierarchyNode).Level == 0 ? 0 : 1;
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
            if (node == null || node.Script == null)
                return;

            if (node.Script == ScriptManager.Instance.ActiveScript && node.Script.IsDirty)
                e.SubItem.Font = Fonts.ActiveAndDirtyScript;//.AddFont(Fonts.ActiveScript);
            else if (node.Script == ScriptManager.Instance.ActiveScript)
                e.SubItem.Font = Fonts.ActiveScript;
            else if (node.Script.IsDirty)
                e.SubItem.Font = Fonts.DirtyScript;//.AddFont(Fonts.DirtyScript);
            else
                e.SubItem.Font = Fonts.Default;
        }

        private void UpdateHierarchy()
        {
            m_Nodes.Clear();

            foreach (var s in ScriptManager.Instance.LoadedScripts)
                m_Nodes.Add(new HierarchyNode(s));

            RefreshTreeListView();
            treeListView.ExpandAll();
        }

        private void RefreshTreeListView()
        {
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
            m_Nodes.Add(node);
            treeListView.Expand(node);
            RefreshTreeListView();

            treeListView.SelectedObject = node;

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        private void OnScriptModified(Script script)
        {
            var node = new HierarchyNode(script);
            var index = script.Index;
            m_Nodes[index] = node;
            RefreshTreeListView();

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        private void OnScriptRemoved(int index)
        {
            var oldSelectedObject = treeListView.SelectedObject;

            m_Nodes.RemoveAt(index);
            RefreshTreeListView();

            if (treeListView.SelectedObject != oldSelectedObject)
                OnCommandSelected?.Invoke(null);

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        private void OnScriptPositioningChanged()
        {
            foreach (var script in ScriptManager.Instance.LoadedScripts)
            {
                var index = m_Nodes.FindIndex(n => n.Script == script);
                m_Nodes.MoveBefore(index, script.Index);
            }

            RefreshTreeListView();
            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        // Will not work in nested scenario
        private void OnCommandAddedToScript(Script script, Command command)
        {
            var scriptNode = m_Nodes.FirstOrDefault(node => node.Script == script);
            scriptNode.Children.Add(new HierarchyNode(command, scriptNode));
            RefreshTreeListView();
        }

        // Will not work in nested scenario
        private void OnCommandRemovedFromScript(Script script, int commandIndex)
        {
            var scriptNode = m_Nodes.FirstOrDefault(node => node.Script == script);
            scriptNode.Children.RemoveAt(commandIndex);
            RefreshTreeListView();
        }

        // Will not work in nested scenario
        private void OnCommandModifiedOnScript(Script script, Command command)
        {
            var scriptNode = m_Nodes.FirstOrDefault(node => node.Script == script);
            var commandIndex = script.Commands.IndexOf(command);
            var commandNode = scriptNode.Children[commandIndex];
            commandNode.Update(command);
            RefreshTreeListView();
        }

        // Will not work in nested scenario
        private void OnCommandInsertedInScript(Script script, Command command, int pos)
        {
            var scriptNode = m_Nodes.FirstOrDefault(n => n.Script == script);
            var node = new HierarchyNode(command, scriptNode);
            scriptNode.Children.Insert(pos, node);
            // Will not work with multi dragging
            RefreshTreeListView();
            treeListView.SelectedObject = node;
        }

        #endregion

        #region Context Menu Items
        private void setActiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedNode = treeListView.SelectedObject as HierarchyNode;
            if (selectedNode == null || selectedNode.Script == null)
                return;

            ScriptManager.Instance.ActiveScript = selectedNode.Script;
            RefreshTreeListView();
        }

        public void newScriptToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ScriptManager.Instance.NewScript();
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
                ScriptManager.Instance.NewScript(selectedNode.Script);
                ScriptManager.Instance.MoveScriptAfter(ScriptManager.Instance.LoadedScripts.Count - 1, selectedNode.Script.Index);
            }
            else if (selectedNode.Command != null)
            {
                var clone = (Command)selectedNode.Command.Clone();
                var parent = selectedNode.Parent;

                // TODO: this will fail if parent is another command
                selectedNode.Parent.Script.InsertCommandAfter(selectedNode.Command, clone);
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
                ScriptManager.Instance.RemoveScript(selectedNode.Script);
            else if (selectedNode.Command != null)
                ScriptManager.Instance.GetScriptFromCommand(selectedNode.Command).RemoveCommand(selectedNode.Command);

            RefreshTreeListView();

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }
        #endregion

        #region Menu Items (save scripts from MainForm)
        public void SaveAllScripts()
        {
            foreach (var script in ScriptManager.Instance)
            {
                if (!script.IsDirty)
                    continue;

                if (script.Path != "")
                    ScriptManager.Instance.SaveScript(script, script.Path);
                else
                    SaveSelectedScriptWithDialog(script, updateUI: false);
            }

            RefreshTreeListView();
        }

        public void SaveSelectedScriptWithDialog(Script script, bool updateUI = true)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.InitialDirectory = MouseRobot.Instance.ProjectPath + "\\" + AssetManager.ScriptFolder;
            saveDialog.Filter = string.Format("Mouse Robot File (*.{0})|*.{0}", FileExtensions.Script);
            saveDialog.Title = "Select a path for script to save.";
            saveDialog.FileName = script.Name + FileExtensions.ScriptD;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                ScriptManager.Instance.SaveScript(ScriptManager.Instance.ActiveScript, saveDialog.FileName);
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
            e.DropSink.CanDropOnItem = false;

            if (targetNode == null || sourceNode == null ||
                targetNode.Script == null && sourceNode.Command == null ||
                targetNode.Command == null && sourceNode.Script == null ||
                targetNode.Script != null && sourceNode.Script != null && e.DropTargetLocation == DropTargetLocation.Item)
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
                    ScriptManager.Instance.MoveScriptBefore(sourceNode.Script.Index, targetNode.Script.Index);
                if (e.DropTargetLocation == DropTargetLocation.BelowItem)
                    ScriptManager.Instance.MoveScriptAfter(sourceNode.Script.Index, targetNode.Script.Index);
            }

            if (targetNode.Command != null && sourceNode.Command != null)
            {
                var targetScript = ScriptManager.Instance.GetScriptFromCommand(targetNode.Command);
                var sourceScript = ScriptManager.Instance.GetScriptFromCommand(sourceNode.Command);

                // Will not work with nesting
                if (e.DropTargetLocation == DropTargetLocation.AboveItem)
                    ScriptManager.Instance.MoveCommandAfter(sourceNode.Command.GetIndex(), targetNode.Command.GetIndex() - 1, sourceScript.Index, targetScript.Index);
                if (e.DropTargetLocation == DropTargetLocation.BelowItem)
                    ScriptManager.Instance.MoveCommandAfter(sourceNode.Command.GetIndex(), targetNode.Command.GetIndex(), sourceScript.Index, targetScript.Index);

                if (e.DropTargetLocation == DropTargetLocation.Item)
                {
                    // Do something here to nest commands
                }
            }
        }

        #endregion

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
            for (int i = 0; i < m_Nodes.Count; i++)
            {
                Debug.Assert(m_Nodes[i].Script == ScriptManager.Instance.LoadedScripts[i],
                    string.Format("Hierarchy script missmatch: {0}:{1}", i,  m_Nodes[i].Value.ToString()));

                // Will not work in nested scenarios
                for (int j = 0; j < m_Nodes[i].Script.Commands.Count(); j++)
                {
                    Debug.Assert(m_Nodes[i].Children[j].Command == ScriptManager.Instance.LoadedScripts[i].Commands.GetChild(j).value,
                        string.Format("Hierarchy command missmatch: {0}:{1}, {2}:{3}", 
                        i, m_Nodes[i].Value.ToString(), j, m_Nodes[i].Script.Commands.GetChild(j).value.ToString()));
                }
            }
#endif
        }
    }
}
