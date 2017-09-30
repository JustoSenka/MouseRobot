using Robot;
using RobotUI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotUI
{
    public partial class TreeViewWindow : DockContent
    {
        public TreeViewWindow()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;

            treeView.Font = Fonts.Default;
            ScriptTreeViewUtils.UpdateTreeView(treeView);
            ScriptTreeViewUtils.UpdateTreeNodeFonts(treeView);

            ScriptManager.Instance.scriptsModified += OnScriptsModified;
        }

        private void OnScriptsModified(Script script, Command command)
        {
            // Add Command to tree view that was probably added to manager while recording
            treeView.Nodes[script.Index].Nodes.Add(command.ToString());
            ScriptTreeViewUtils.UpdateTreeNodeFonts(treeView);
        }

        #region TreeView Drag and Drop
        private void treeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
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
