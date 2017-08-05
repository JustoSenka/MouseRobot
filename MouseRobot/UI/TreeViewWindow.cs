using Robot;
using RobotUI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
