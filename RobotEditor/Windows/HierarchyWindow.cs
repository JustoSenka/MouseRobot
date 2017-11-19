using RobotEditor.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using RobotRuntime;
using RobotRuntime.Utils.Win32;
using Robot;

namespace RobotEditor
{
    public partial class HierarchyWindow : DockContent
    {
        public event Action<Command> OnCommandSelected;

        public HierarchyWindow()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
            treeView.NodeMouseClick += (sender, args) => treeView.SelectedNode = args.Node;

            treeView.Font = Fonts.Default;
            ScriptTreeViewUtils.UpdateTreeView(treeView);
            ScriptTreeViewUtils.UpdateTreeNodeFonts(treeView);

            ScriptManager.Instance.CommandAddedToScript += OnCommandAddedToScript;
            ScriptManager.Instance.CommandModifiedOnScript += OnCommandModifiedOnScript;
            ScriptManager.Instance.scriptLoaded += OnScriptLoaded;
            ScriptManager.Instance.scriptRemoved += OnScriptRemoved;
        }

        private void OnCommandAddedToScript(Script script, Command command)
        {
            var node = new TreeNode(command.ToString());
            node.ImageIndex = 1;
            node.SelectedImageIndex = 1;

            treeView.Nodes[script.Index].Nodes.Add(node);
            if (treeView.Nodes[script.Index].GetNodeCount(false) == 1)
                treeView.Nodes[script.Index].Expand();

            ScriptTreeViewUtils.UpdateTreeNodeFonts(treeView);
        }

        private void OnCommandModifiedOnScript(Script script, Command command)
        {
            var commandIndex = script.Commands.IndexOf(command);
            treeView.Nodes[script.Index].Nodes[commandIndex].Text = command.ToString();
            ScriptTreeViewUtils.UpdateTreeNodeFonts(treeView);
        }

        private void OnScriptLoaded(Script script)
        {
            var node = treeView.FindChildRegex("^" + script.Name + @" ?\*?$");
            if (node == null)
                ScriptTreeViewUtils.AddExistingScriptToTreeView(treeView, script);
            else
                ScriptTreeViewUtils.UpdateExistingScriptNode(treeView, script);

            treeView.Nodes[script.Index].Expand();

            ScriptTreeViewUtils.UpdateTreeNodeFonts(treeView);
        }

        private void OnScriptRemoved(int index)
        {
            if (treeView.SelectedNode.Level >= 1 && treeView.SelectedNode.Parent.Index == index)
                OnCommandSelected?.Invoke(null);

            treeView.Nodes[index].Remove();
        }

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

        #region Context Menu Items
        private void setActiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.SetSelectedScriptActive(treeView);
        }

        private void newScriptToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.NewScript(treeView);
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.ShowSelectedTreeViewItemInExplorer(treeView);
        }

        private void duplicateToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DuplicateSelectedTreeViewItem(treeView);
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ScriptTreeViewUtils.DeleteSelectedTreeViewItem(treeView);
        }
        #endregion
    }
}
