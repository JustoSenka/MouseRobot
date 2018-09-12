using Robot;
using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotRuntime;
using RobotRuntime.Scripts;
using RobotRuntime.Tests;
using RobotRuntime.Utils;
using System;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor
{
    public partial class AssetsWindow : DockContent, IAssetsWindow
    {
        public event Action AssetSelected;

        private IAssetManager AssetManager;
        private IScriptManager ScriptManager;
        private ITestFixtureManager TestFixtureManager;
        private IPluginManager PluginManager;
        public AssetsWindow(IAssetManager AssetManager, IScriptManager ScriptManager, ITestFixtureManager TestFixtureManager, IPluginManager PluginManager)
        {
            this.AssetManager = AssetManager;
            this.ScriptManager = ScriptManager;
            this.TestFixtureManager = TestFixtureManager;
            this.PluginManager = PluginManager;

            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
            treeView.Font = Fonts.Default;
            treeView.NodeMouseClick += (sender, args) => treeView.SelectedNode = args.Node;

            treeView.AfterRename += OnAfterRenameNode;

            AssetManager.RefreshFinished += OnRefreshFinished;
            AssetManager.AssetCreated += OnAssetCreated;
            AssetManager.AssetDeleted += OnAssetDeleted;
        }

        public Asset GetSelectedAsset()
        {
            return AssetManager.GetAsset(treeView.SelectedNode.Parent.Text, treeView.SelectedNode.Text);
        }

        private void OnAfterRenameNode(NodeLabelEditEventArgs e)
        {
            var asset = AssetManager.GetAsset(e.Node.Parent.Text, e.Node.Text);
            var newPath = asset.Path.Replace("\\" + Paths.GetName(asset.Path), "\\" + e.Label);
            AssetManager.RenameAsset(asset.Path, newPath);
        }

        private void OnAssetDeleted(string path)
        {
            treeView.FindNode(path).Remove();
        }

        private void OnAssetCreated(string path)
        {
            var folderNode = treeView.FindChild(Paths.GetFolder(path));
            var node = new TreeNode(Paths.GetName(path));
            folderNode.Nodes.Add(node);
            UpdateIcons();
        }

        private void OnRefreshFinished()
        {
            treeView.Nodes.Clear();

            var scriptNode = new TreeNode(Paths.ScriptFolder);
            var imageNode = new TreeNode(Paths.ImageFolder);
            var pluginNode = new TreeNode(Paths.PluginFolder);
            var testsNode = new TreeNode(Paths.TestsFolder);

            treeView.Nodes.Add(scriptNode);
            treeView.Nodes.Add(imageNode);
            treeView.Nodes.Add(pluginNode);
            treeView.Nodes.Add(testsNode);

            foreach (var asset in AssetManager.Assets)
            {
                TreeNode assetNode = new TreeNode(asset.Name)
                {
                    ImageIndex = 0,
                    SelectedImageIndex = 0
                };

                if (asset.Path.EndsWith(FileExtensions.Image))
                    imageNode.Nodes.Add(assetNode);

                else if (asset.Path.EndsWith(FileExtensions.Script))
                    scriptNode.Nodes.Add(assetNode);

                else if (asset.Path.EndsWith(FileExtensions.Plugin))
                    pluginNode.Nodes.Add(assetNode);

                else if (asset.Path.EndsWith(FileExtensions.Test))
                    testsNode.Nodes.Add(assetNode);

                else
                    Logger.Log(LogType.Error, "Unknown item appeared in asset database:" + asset.Path);
            }

            treeView.ExpandAll();
            UpdateIcons();
        }

        private void UpdateIcons()
        {
            UpdateIconForFolder("", 0);
            UpdateIconForFolder(Paths.ScriptFolder, 1);
            UpdateIconForFolder(Paths.ImageFolder, 2);
            UpdateIconForFolder(Paths.PluginFolder, 3);
            UpdateIconForFolder(Paths.TestsFolder, 3);
        }

        private void UpdateIconForFolder(string folder, int iconIndex)
        {
            var nodeCollecion = folder == "" ? treeView.Nodes : treeView.FindChild(folder).Nodes;
            foreach (TreeNode node in nodeCollecion)
            {
                node.ImageIndex = iconIndex;
                node.SelectedImageIndex = iconIndex;
            }
        }

        private void treeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (treeView.SelectedNode == null || treeView.SelectedNode.Level != 1 ||
                treeView.SelectedNode.Parent.Text != Paths.ScriptFolder &&
                treeView.SelectedNode.Parent.Text != Paths.TestsFolder)
                return;

            var asset = AssetManager.GetAsset(treeView.SelectedNode.Parent.Text, treeView.SelectedNode.Text);

            if (asset.HoldsTypeOf(typeof(Script)))
            {
                if (!ScriptManager.LoadedScripts.Any(s => s.Name == asset.Name))
                    ScriptManager.LoadScript(asset.Path);
            }
            else if (asset.HoldsTypeOf(typeof(LightTestFixture)))
            {
                if (!TestFixtureManager.Contains(asset.Name))
                {
                    var lightTestFixture = asset.Importer.Load<LightTestFixture>();
                    if (lightTestFixture != null)
                    {
                        lightTestFixture.Name = asset.Name;
                        var fixture = TestFixtureManager.NewTestFixture(lightTestFixture);
                        fixture.Path = asset.Path;
                    }
                }
                // TODO: Send some message to main form to give focus to window is TestFixture is already open
            }
        }

        private void reloadScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null || treeView.SelectedNode.Level != 1 || treeView.SelectedNode.Parent.Text != Paths.ScriptFolder)
                return;

            var asset = AssetManager.GetAsset(treeView.SelectedNode.Parent.Text, treeView.SelectedNode.Text);
            ScriptManager.LoadScript(asset.Path);
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView.SelectedNode != null && treeView.SelectedNode.Level == 1)
                AssetSelected?.Invoke();
        }

        private void recompileScriptsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PluginManager.CompileScriptsAndReloadUserDomain();
        }

        #region Context Menu Items

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AssetManager.Refresh();
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            var path = Environment.CurrentDirectory + "\\";
            if (treeView.SelectedNode.Level == 0)
            {
                path += treeView.SelectedNode.Text;
            }
            else if (treeView.SelectedNode.Level == 1)
            {
                path += AssetManager.GetAsset(treeView.SelectedNode.Parent.Text, treeView.SelectedNode.Text).Path;
            }

            System.Diagnostics.Process.Start("explorer.exe", "/select, " + path);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode.Level == 1)
            {
                var asset = AssetManager.GetAsset(treeView.SelectedNode.Parent.Text, treeView.SelectedNode.Text);
                AssetManager.DeleteAsset(asset.Path);
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
