#define ENABLE_UI_TESTING

using RobotEditor.Utils;
using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using RobotRuntime;
using Robot;
using RobotEditor.Scripts;
using BrightIdeasSoftware;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

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
            //ScriptTreeViewUtils.UpdateTreeView(treeListView);
            //ScriptTreeViewUtils.UpdateTreeNodeFonts(treeView);

            ScriptManager.Instance.CommandAddedToScript += OnCommandAddedToScript;
            ScriptManager.Instance.CommandModifiedOnScript += OnCommandModifiedOnScript;

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

            nameColumn.Width = treeListView.Width;
            treeListView.Columns.Add(nameColumn);
        }

        private void UpdateHierarchy()
        {
            m_Nodes.Clear();

            foreach (var s in ScriptManager.Instance.LoadedScripts)
                m_Nodes.Add(new HierarchyNode(s));

            RefreshTreeListView();
            treeListView.ExpandAll();
        }

        #region ScriptManager Callbacks

        private void OnScriptLoaded(Script script)
        {
            var node = new HierarchyNode(script);
            m_Nodes.Add(node);
            treeListView.Expand(node);
            RefreshTreeListView();

            treeListView.SelectedObject = node;

            //ScriptTreeViewUtils.UpdateTreeNodeFonts(treeView);
            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        private void OnScriptModified(Script script)
        {
            var node = new HierarchyNode(script);
            var index = script.Index;
            m_Nodes[index] = node;
            RefreshTreeListView();

            //ScriptTreeViewUtils.UpdateTreeNodeFonts(treeView);
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

            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        private void OnCommandAddedToScript(Script script, Command command)
        {
            var scriptNode = m_Nodes.FirstOrDefault(node => node.Script == script);
            scriptNode.Children.Add(new HierarchyNode(command, scriptNode));
            RefreshTreeListView();

            //ScriptTreeViewUtils.UpdateTreeNodeFonts(treeView);
            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

        private void OnCommandModifiedOnScript(Script script, Command command)
        {
            var scriptNode = m_Nodes.FirstOrDefault(node => node.Script == script);
            var commandIndex = script.Commands.IndexOf(command);
            var commandNode = scriptNode.Children[commandIndex];
            commandNode.Update(command);
            RefreshTreeListView();

            //ScriptTreeViewUtils.UpdateTreeNodeFonts(treeView);
            ASSERT_TreeViewIsTheSameAsInScriptManager();
        }

#endregion

        private void RefreshTreeListView()
        {
            treeListView.Roots = m_Nodes;
            treeListView.Refresh();
            for (int i = 0; i < treeListView.Items.Count; ++i)
            {
                treeListView.Items[i].ImageIndex = 0;
            }
        }

        //-----------------------------------

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level >= 1)
            {
                var script = ScriptManager.Instance.LoadedScripts[e.Node.Parent.Index];
                var command = script.Commands.GetChild(e.Node.Index).value;
                OnCommandSelected?.Invoke(command);
            }
            else
            {
                OnCommandSelected?.Invoke(null);
            }
        }



        /*
        #region TreeView Drag and Drop
        private void treeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            Point targetPoint = treeView.PointToClient(WinAPI.GetCursorPosition());
            treeView.SelectedNode = treeView.GetNodeAt(targetPoint);

            DoDragDrop(e.Item, DragDropEffects.All);
        }

        private void treeView_DragDrop(object sender, DragEventArgs e)
        {
            ScriptTreeViewUtils.TreeView_DragDrop(treeView, e);
        }

        private void treeView_DragOver(object sender, DragEventArgs e)
        {
            ScriptTreeViewUtils.TreeView_DragOver(treeView, e);
        }
        #endregion
        */
        #region Context Menu Items
        private void setActiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedNode = treeListView.SelectedObject as HierarchyNode;
            if (selectedNode == null || selectedNode.Script == null)
                return;

            ScriptManager.Instance.ActiveScript = selectedNode.Script;
            //UpdateTreeNodeFonts(treeView);
        }

        private void newScriptToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ScriptManager.Instance.NewScript();
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedNode = treeListView.SelectedObject as HierarchyNode;
            if (selectedNode == null || selectedNode.Script == null)
                return;

            Process.Start("explorer.exe", "/select, " + selectedNode.Script.Path);
        }

        // no callback yet for moveafter
        private void duplicateToolStripMenuItem1_Click(object sender, EventArgs e)
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

            treeListView.Focus();
            //UpdateTreeNodeFonts(treeView);
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //ScriptTreeViewUtils.DeleteSelectedTreeViewItem(treeView);
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
