using Robot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotUI
{
    public partial class AssetsWindow : DockContent
    {
        public AssetsWindow()
        {
            InitializeComponent();
            AssetManager.Instance.RefreshFinished += RefreshFinished;

            AssetManager.Instance.Refresh();
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
    }
}
