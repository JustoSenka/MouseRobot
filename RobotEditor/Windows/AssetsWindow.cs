using Robot;
using RobotRuntime;
using System;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor
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

            treeView.AfterRename += OnAfterRenameNode;

            AssetManager.Instance.RefreshFinished += OnRefreshFinished;
            AssetManager.Instance.AssetCreated += OnAssetCreated;
            AssetManager.Instance.AssetDeleted += OnAssetDeleted;

            AssetManager.Instance.Refresh();
        }

        public Asset GetSelectedAsset()
        {
            return AssetManager.Instance.GetAsset(treeView.SelectedNode.Parent.Text, treeView.SelectedNode.Text);
        }

        private void OnAfterRenameNode(NodeLabelEditEventArgs e)
        {
            var asset = AssetManager.Instance.GetAsset(e.Node.Parent.Text, e.Node.Text);
            var newPath = asset.Path.Replace("\\" + Commons.GetName(asset.Path), "\\" + e.Label);
            AssetManager.Instance.RenameAsset(asset.Path, newPath);
        }

        private void OnAssetDeleted(string path)
        {
            treeView.FindNode(path).Remove();
        }

        private void OnAssetCreated(string path)
        {
            var folderNode = treeView.FindChild(Commons.GetFolder(path));
            var node = new TreeNode(Commons.GetName(path));
            folderNode.Nodes.Add(node);
        }

        private void OnRefreshFinished()
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

        private void treeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (treeView.SelectedNode == null || treeView.SelectedNode.Level != 1 || treeView.SelectedNode.Parent.Text != AssetManager.ScriptFolder)
                return;

            var asset = AssetManager.Instance.GetAsset(treeView.SelectedNode.Parent.Text, treeView.SelectedNode.Text);
            if (!ScriptManager.Instance.LoadedScripts.Any(s => s.Name == asset.Name))
                ScriptManager.Instance.LoadScript(asset.Path);
        }

        #region Context Menu Items

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AssetManager.Instance.Refresh();
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView.SelectedNode != null && treeView.SelectedNode.Level == 1)
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

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode.Level == 1)
            {
                var asset = AssetManager.Instance.GetAsset(treeView.SelectedNode.Parent.Text, treeView.SelectedNode.Text);
                AssetManager.Instance.DeleteAsset(asset.Path);
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode.Level == 1)
                treeView.SelectedNode.BeginEdit();
        }

        #endregion
    }
}
