using Robot;
using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Assets;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using RobotRuntime.Utils;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor
{
    public partial class AssetsWindow : DockContent, IAssetsWindow
    {
        public event Action AssetSelected;

        private IAssetManager AssetManager;
        private IHierarchyManager RecordingManager;
        private ITestFixtureManager TestFixtureManager;
        private IScriptManager ScriptManager;
        private ISolutionManager SolutionManager;
        private ICodeEditor CodeEditor;
        private ILogger Logger;
        private IScriptTemplates ScriptTemplates;
        public AssetsWindow(IAssetManager AssetManager, IHierarchyManager RecordingManager, ITestFixtureManager TestFixtureManager,
            IScriptManager ScriptManager, ISolutionManager SolutionManager, ICodeEditor CodeEditor, ILogger Logger, IScriptTemplates ScriptTemplates)
        {
            this.AssetManager = AssetManager;
            this.RecordingManager = RecordingManager;
            this.TestFixtureManager = TestFixtureManager;
            this.ScriptManager = ScriptManager;
            this.SolutionManager = SolutionManager;
            this.CodeEditor = CodeEditor;
            this.Logger = Logger;
            this.ScriptTemplates = ScriptTemplates;

            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
            treeView.Font = Fonts.Default;
            treeView.NodeMouseClick += (sender, args) => treeView.SelectedNode = args.Node;

            treeView.AfterRename += OnAfterRenameNode;

            AssetManager.RefreshFinished += OnRefreshFinished;
            AssetManager.AssetCreated += OnAssetCreated;
            AssetManager.AssetDeleted += OnAssetDeleted;

            treeView.HandleCreated += OnRefreshFinished;
            contextMenuStrip.HandleCreated += (s, e) => AddMenuItemsForScriptTemplates(contextMenuStrip, "addScriptToolStripMenuItem");
        }

        public void AddMenuItemsForScriptTemplates(ToolStrip menuStrip, string menuItemName)
        {
            menuStrip.BeginInvoke(new MethodInvoker(() =>
            {
                var menuItem = (ToolStripMenuItem)menuStrip.Items.Find(menuItemName, true)[0];

                foreach (var name in ScriptTemplates.TemplateNames)
                {
                    var item = new ToolStripMenuItem(name);
                    item.Click += (sender, eventArgs) =>
                    {
                        var script = ScriptTemplates.GetTemplate(name);
                        var fileName = ScriptTemplates.GetTemplateFileName(name);
                        var filePath = Path.Combine(Paths.ScriptPath, fileName + ".cs");
                        filePath = Paths.GetUniquePath(filePath);
                        AssetManager.CreateAsset(script, filePath);
                    };
                    menuItem.DropDownItems.Add(item);
                }
            }));
        }

        public Asset GetSelectedAsset()
        {
            var asset = AssetManager.GetAsset(treeView.SelectedNode.Parent.Text, treeView.SelectedNode.Text);

            if (asset == null)
                Logger.Logi(LogType.Error,
                    "AssetWindow has asset selected, but the AssetManager has no such asset registered. Something went wrong, please report a bug.",
                    $"Asset folder: '{treeView.SelectedNode.Parent.Text}' Asset Name: '{treeView.SelectedNode.Text}'");

            return asset;
        }

        private void OnAfterRenameNode(NodeLabelEditEventArgs e)
        {
            var asset = AssetManager.GetAsset(e.Node.Parent.Text, e.Node.Text);
            var newPath = asset.Path.Replace("\\" + Paths.GetName(asset.Path), "\\" + e.Label);
            AssetManager.RenameAsset(asset.Path, newPath);
        }

        private void OnAssetDeleted(string path)
        {
            treeView.BeginInvokeIfCreated(new MethodInvoker(() =>
            {
                var node = treeView.FindNode(path);
                if (Logger.AssertIf(node == null,
                    @"Asset was deleted from database, but it was never added to AssetWindow TreeView. Please report a bug if this is a constant thing."))
                    return;

                treeView.FindNode(path).Remove();
            }));
        }

        private void OnAssetCreated(string path)
        {
            treeView.BeginInvokeIfCreated(new MethodInvoker(() =>
            {
                var folderNode = treeView.FindChild(Paths.GetFolder(path));
                var node = new TreeNode(Paths.GetName(path));
                folderNode.Nodes.Add(node);
                UpdateIcons();
            }));
        }

        private void OnRefreshFinished(object sender, EventArgs args)
        {
            OnRefreshFinished();
        }

        private void OnRefreshFinished()
        {
            treeView.BeginInvokeIfCreated(new MethodInvoker(() =>
            {
                treeView.Nodes.Clear();

                var recordingNode = new TreeNode(Paths.RecordingFolder);
                var imageNode = new TreeNode(Paths.ImageFolder);
                var scriptNode = new TreeNode(Paths.ScriptFolder);
                var testsNode = new TreeNode(Paths.TestsFolder);
                var dllNode = new TreeNode(Paths.PluginFolder);

                treeView.Nodes.Add(recordingNode);
                treeView.Nodes.Add(imageNode);
                treeView.Nodes.Add(scriptNode);
                treeView.Nodes.Add(testsNode);
                treeView.Nodes.Add(dllNode);

                foreach (var asset in AssetManager.Assets)
                {
                    TreeNode assetNode = new TreeNode(asset.Name)
                    {
                        ImageIndex = 0,
                        SelectedImageIndex = 0
                    };

                    if (asset.Path.EndsWith(FileExtensions.ImageD))
                        imageNode.Nodes.Add(assetNode);

                    else if (asset.Path.EndsWith(FileExtensions.RecordingD))
                        recordingNode.Nodes.Add(assetNode);

                    else if (asset.Path.EndsWith(FileExtensions.ScriptD))
                        scriptNode.Nodes.Add(assetNode);

                    else if (asset.Path.EndsWith(FileExtensions.TestD))
                        testsNode.Nodes.Add(assetNode);

                    else if (asset.Path.EndsWith(FileExtensions.Dll) || asset.Path.EndsWith(FileExtensions.ExeD))
                        dllNode.Nodes.Add(assetNode);

                    else
                        Logger.Logi(LogType.Error, "Unknown item appeared in asset database:" + asset.Path);
                }

                treeView.Refresh();
                treeView.ExpandAll();
                UpdateIcons();
            }));
        }

        private void UpdateIcons()
        {
            UpdateIconForFolder("", 0);
            UpdateIconForFolder(Paths.RecordingFolder, 1);
            UpdateIconForFolder(Paths.ImageFolder, 2);
            UpdateIconForFolder(Paths.ScriptFolder, 3);
            UpdateIconForFolder(Paths.TestsFolder, 3);
            UpdateIconForFolder(Paths.PluginFolder, 3);
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
            if (treeView.SelectedNode == null || treeView.SelectedNode.Level != 1)
                return;

            var asset = AssetManager.GetAsset(treeView.SelectedNode.Parent.Text, treeView.SelectedNode.Text);
            if (Logger.AssertIf(asset == null, $"Asset in database was not found but is visible in Assets Window: {treeView.SelectedNode.Text}. Please report a bug."))
                return;

            if (asset.HoldsTypeOf(typeof(Recording)))
            {
                if (!RecordingManager.LoadedRecordings.Any(s => s.Name == asset.Name))
                    RecordingManager.LoadRecording(asset.Path);
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
            else if (asset.Importer.GetType() == typeof(ScriptImporter))
            {
                Task.Run(() => CodeEditor.FocusFile(asset.Importer.Path));
            }
        }

        private void reloadRecordingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null || treeView.SelectedNode.Level != 1 || treeView.SelectedNode.Parent.Text != Paths.RecordingFolder)
                return;

            var asset = AssetManager.GetAsset(treeView.SelectedNode.Parent.Text, treeView.SelectedNode.Text);
            RecordingManager.LoadRecording(asset.Path);
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView.SelectedNode != null && treeView.SelectedNode.Level == 1)
                AssetSelected?.Invoke();
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

        private void recompileRecordingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptManager.CompileScriptsAndReloadUserDomain();
        }

        private void regenerateSolutionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SolutionManager.GenerateNewProject();
            SolutionManager.GenerateNewSolution();
        }

        #endregion
    }
}
