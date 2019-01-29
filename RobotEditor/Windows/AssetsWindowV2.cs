using BrightIdeasSoftware;
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
    public partial class AssetsWindowV2 : DockContent, IAssetsWindow
    {
        public event Action AssetSelected;

        private TreeNode<Asset> m_AssetTree = new TreeNode<Asset>();

        private IAssetManager AssetManager;
        private IHierarchyManager RecordingManager;
        private ITestFixtureManager TestFixtureManager;
        private IScriptManager ScriptManager;
        private ISolutionManager SolutionManager;
        private ICodeEditor CodeEditor;
        private ILogger Logger;
        private IScriptTemplates ScriptTemplates;
        public AssetsWindowV2(IAssetManager AssetManager, IHierarchyManager RecordingManager, ITestFixtureManager TestFixtureManager,
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
            treeListView.Font = Fonts.Default;

            treeListView.HandleCreated += OnRefreshFinished;
            AssetManager.RefreshFinished += () => OnRefreshFinished(this, null);

            contextMenuStrip.HandleCreated += (s, e) => AddMenuItemsForScriptTemplates(contextMenuStrip, "addScriptToolStripMenuItem");

            CreateColumns();
        }

        public void CreateColumns()
        {
            treeListView.CanExpandGetter = x => (x as TreeNode<Asset>).Count() > 0;
            treeListView.ChildrenGetter = x => x as TreeNode<Asset>; // Becasue TreeNode : IEnumerable

            var nameColumn = new OLVColumn("Name", "Name");
            nameColumn.AspectGetter = x => (x as TreeNode<Asset>).value.ToString();

            nameColumn.ImageGetter += delegate (object x)
            {
                var imageListIndex = -1;
                var node = (TreeNode<Asset>)x;
                imageListIndex = Directory.Exists(node.value.Path) ? 0 : imageListIndex;
                imageListIndex = node.value.Path.EndsWith(FileExtensions.RecordingD) ? 1 : imageListIndex;
                imageListIndex = node.value.Path.EndsWith(FileExtensions.ImageD) ? 2 : imageListIndex;
                imageListIndex = node.value.Path.EndsWith(FileExtensions.ScriptD) ? 3 : imageListIndex;
                imageListIndex = node.value.Path.EndsWith(FileExtensions.TestD) ? 3 : imageListIndex;
                imageListIndex = node.value.Path.EndsWith(FileExtensions.DllD) ? 3 : imageListIndex;
                return imageListIndex;
            };

            treeListView.UseCellFormatEvents = true;

            treeListView.IsSimpleDragSource = true;
            treeListView.IsSimpleDropSink = true;

            treeListView.TreeColumnRenderer.IsShowLines = false;

            nameColumn.Width = treeListView.Width;
            treeListView.Columns.Add(nameColumn);
        }

        private void OnRefreshFinished(object sender, EventArgs e)
        {
            treeListView.BeginInvokeIfCreated(new MethodInvoker(() =>
            {
                m_AssetTree.Clear();
                m_AssetTree.AddChild(AssetManager.GetAsset(Paths.AssetsFolder));

                foreach (var asset in AssetManager.Assets)
                {
                    var assetPath = asset.Importer.Path;
                    var allDirElements = Paths.GetPathDirectoryElementsWtihFileName(assetPath);
                    var allElementPaths = Paths.JoinDirectoryElementsIntoPaths(allDirElements);
                    foreach (var path in allElementPaths.Skip(1)) // Skipping Assets folder for performance reasons
                    {
                        var c = m_AssetTree.FindNodeFromPath(path);
                        if (c == null) // Only add if asset or directory if it does not exist
                        {
                            var intermediateAsset = AssetManager.GetAsset(path);
                            if (Logger.AssertIf(intermediateAsset == null, "Asset Manager does not know about this asset: " + path + " but Assets Window tried to draw it."))
                                continue;

                            m_AssetTree.AddChildAtPath(path, intermediateAsset);
                        }
                    }
                }

                treeListView.Roots = m_AssetTree;
                treeListView.Refresh();
            }));
        }

        private void treeListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (treeListView.SelectedObject == null)
                return;

            var assetNode = treeListView.SelectedObject as TreeNode<Asset>;
            if (Logger.AssertIf(assetNode == null, $"Asset in database was not found but is visible in Assets Window: {treeListView.SelectedObject}. Please report a bug."))
                return;

            var asset = assetNode.value;
            if (Logger.AssertIf(asset == null, $"Asset Node is created for tree view, but its value (Asset) is null: {assetNode}. Please report a bug."))
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

        private void treeListView_SelectionChanged(object sender, EventArgs e)
        {
            var assetNode = treeListView.SelectedObject as TreeNode<Asset>;
            var asset = assetNode?.value;
            if (asset == null)
                return;

            AssetSelected?.Invoke();
        }

        private void treeListView_ModelDropped(object sender, ModelDropEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void treeListView_ModelCanDrop(object sender, ModelDropEventArgs e)
        {
            throw new NotImplementedException();
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
                        var filePath = Path.Combine(Paths.AssetsPath, fileName + ".cs");
                        filePath = Paths.GetUniquePath(filePath);
                        AssetManager.CreateAsset(script, filePath);
                    };
                    menuItem.DropDownItems.Add(item);
                }
            }));
        }

        public Asset GetSelectedAsset()
        {
            if (treeListView.SelectedObject == null)
                return null;

            var asset = treeListView.SelectedObject as TreeNode<Asset>;

            if (asset == null || asset.value == null)
                Logger.Logi(LogType.Error,
                    "AssetWindow has asset selected, but the AssetManager has no such asset registered. Something went wrong, please report a bug.",
                    $"Selected Object: '{treeListView.SelectedObject}'");

            return asset.value;
        }

        #region Context Menu Items

        private void reloadRecordingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var assetNode = treeListView.SelectedObject as TreeNode<Asset>;
            var asset = assetNode?.value;
            if (asset == null || !asset.HoldsTypeOf(typeof(Recording)))
                return;

            RecordingManager.LoadRecording(asset.Path);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AssetManager.Refresh();
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var assetNode = treeListView.SelectedObject as TreeNode<Asset>;
            var asset = assetNode?.value;
            if (asset == null)
                return;

            var path = Path.Combine(Environment.CurrentDirectory, asset.Path);
            System.Diagnostics.Process.Start("explorer.exe", "/select, " + path);
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {

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

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var assetNode = treeListView.SelectedObject as TreeNode<Asset>;
            var asset = assetNode?.value;
            if (asset == null)
                return;

            AssetManager.DeleteAsset(asset.Path);
        }

        #endregion
    }
}
