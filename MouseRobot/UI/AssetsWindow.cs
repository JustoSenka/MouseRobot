using Robot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotUI
{
    public partial class AssetsWindow : DockContent
    {
        public event Action AssetSelected;

        public AssetsWindow()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
            treeView.Font = Fonts.Default;
            treeView.NodeMouseClick += (sender, args) => treeView.SelectedNode = args.Node;

            AssetManager.Instance.RefreshFinished += RefreshFinished;

            AssetManager.Instance.Refresh();
        }

        public Asset GetSelectedAsset()
        {
            return AssetManager.Instance.GetAsset(treeView.SelectedNode.Parent.Text, treeView.SelectedNode.Text);
        }

        private void RefreshFinished()
        {
            treeView.Nodes.Clear();

            var scriptNode = new TreeNode(AssetManager.ScriptFolder);
            var imageNode = new TreeNode(AssetManager.ImageFolder);

            treeView.Nodes.Add(scriptNode);
            treeView.Nodes.Add(imageNode);

            foreach (var asset in AssetManager.Instance.Assets)
            {
                TreeNode assetNode = new TreeNode(asset.Name);
                assetNode.ImageIndex = 0;
                assetNode.SelectedImageIndex = 0;

                if (asset.Path.EndsWith(FileExtensions.Image))
                    imageNode.Nodes.Add(assetNode);

                else if (asset.Path.EndsWith(FileExtensions.Script))
                    scriptNode.Nodes.Add(assetNode);

                else
                    throw new Exception("Unknown item appeared in asset database.");
            }

            treeView.ExpandAll();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AssetManager.Instance.Refresh();
        }

        #region Context Menu Items
        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView.SelectedNode.Level == 1)
                AssetSelected?.Invoke();
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            var path = Application.StartupPath + "\\";
            if (treeView.SelectedNode.Level == 0)
            {
                path += treeView.SelectedNode.Text;
            }
            else if (treeView.SelectedNode.Level == 1)
            {
                path += AssetManager.Instance.GetAsset(treeView.SelectedNode.Parent.Text, treeView.SelectedNode.Text).Path;
            }

            System.Diagnostics.Process.Start("explorer.exe", "/select, " + path);
        }
        #endregion

        private void treeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (treeView.SelectedNode == null || treeView.SelectedNode.Level != 1 || treeView.SelectedNode.Parent.Text != AssetManager.ScriptFolder)
                return;

            var asset = AssetManager.Instance.GetAsset(treeView.SelectedNode.Parent.Text, treeView.SelectedNode.Text);
            if (!ScriptManager.Instance.LoadedScripts.Any(s => s.Name == asset.Name))
                ScriptManager.Instance.LoadScript(asset.Path);

        }
    }
}
