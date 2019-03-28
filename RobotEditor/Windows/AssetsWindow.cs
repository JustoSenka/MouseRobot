#define ENABLE_UI_TESTING

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
    public partial class AssetsWindow : DockContent, IAssetsWindow
    {
        public event Action AssetSelected;

        public ToolStrip ToolStrip { get { return toolStrip; } }

        private TreeNode<Asset> m_AssetTree = new TreeNode<Asset>();

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
            treeListView.Font = Fonts.Default;

            AssetManager.RefreshFinished += OnRefreshFinished;
            AssetManager.AssetCreated += OnAssetCreated;
            AssetManager.AssetDeleted += OnAssetDeleted;
            AssetManager.AssetRenamed += OnAssetRenamed;

            treeListView.HandleCreated += ForceRefresh;

            contextMenuStrip.HandleCreated += (s, e) => AddMenuItemsForScriptTemplates(contextMenuStrip, "addScriptToolStripMenuItem");

            CreateColumns();
        }

        public void CreateColumns()
        {
            treeListView.CanExpandGetter = x => (x as TreeNode<Asset>).Count() > 0;
            treeListView.ChildrenGetter = x => x as TreeNode<Asset>; // Becasue TreeNode : IEnumerable

            var nameColumn = new OLVColumn("Name", "Name");
            nameColumn.AspectGetter = x => Path.GetFileName((x as TreeNode<Asset>).value.Path);

            nameColumn.ImageGetter += delegate (object x)
            {
                var imageListIndex = -1;
                var node = (TreeNode<Asset>)x;
                if (!node.value.Importer.LoadingFailed)
                {
                    imageListIndex = Paths.IsDirectory(node.value.Path) ? 1 : imageListIndex;
                    imageListIndex = node.value.Path.EndsWith(FileExtensions.RecordingD) ? 2 : imageListIndex;
                    imageListIndex = node.value.Path.EndsWith(FileExtensions.ImageD) ? 3 : imageListIndex;
                    imageListIndex = node.value.Path.EndsWith(FileExtensions.ScriptD) ? 4 : imageListIndex;
                    imageListIndex = node.value.Path.EndsWith(FileExtensions.TestD) ? 5 : imageListIndex;
                    imageListIndex = node.value.Path.EndsWith(FileExtensions.DllD) ? 6 : imageListIndex;
                }
                else
                    imageListIndex = 0;

                return imageListIndex;
            };

            nameColumn.Sortable = true;
            treeListView.Sorting = SortOrder.Ascending;

            treeListView.UseCellFormatEvents = true;

            treeListView.IsSimpleDragSource = true;
            treeListView.IsSimpleDropSink = true;

            treeListView.TreeColumnRenderer.IsShowLines = false;

            treeListView.LabelEdit = true;
            treeListView.AfterLabelEdit += OnAfterLabelEdit;

            nameColumn.Width = treeListView.Width;
            treeListView.Columns.Add(nameColumn);
        }

        #region AssetManager Callbacks

        private void ForceRefresh(object sender, EventArgs e)
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

                OnRefreshFinished();
            }));
        }

        private void OnRefreshFinished()
        {
            treeListView.InvokeIfCreated(new MethodInvoker(() =>
            {
                treeListView.Roots = m_AssetTree;
                treeListView.Sort(0);
                treeListView.Refresh();
                treeListView.Expand(m_AssetTree.GetChild(0));

                ASSERT_TreeViewIsTheSameAsInRecordingManager();
            }));
        }

        private void OnAssetCreated(string path)
        {
            treeListView.InvokeIfCreated(new MethodInvoker(() =>
            {
                var asset = AssetManager.GetAsset(path);
                var assetPath = asset.Importer.Path;
                var allDirElements = Paths.GetPathDirectoryElementsWtihFileName(assetPath);
                var allElementPaths = Paths.JoinDirectoryElementsIntoPaths(allDirElements);
                foreach (var elementPath in allElementPaths)
                {
                    var c = m_AssetTree.FindNodeFromPath(elementPath);
                    if (c == null) // Only add if asset or directory if it does not exist
                    {
                        var intermediateAsset = AssetManager.GetAsset(elementPath);
                        if (Logger.AssertIf(intermediateAsset == null, "Asset Manager does not know about this asset: " + elementPath + " but Assets Window tried to draw it."))
                            continue;

                        m_AssetTree.AddChildAtPath(elementPath, intermediateAsset);
                    }
                }
                OnRefreshFinished();
            }));
        }

        private void OnAssetDeleted(string path)
        {
            treeListView.InvokeIfCreated(new MethodInvoker(() =>
            {
                var node = m_AssetTree.FindNodeFromPath(path);
                if (Logger.AssertIf(node == null, "Could not find node in UI in OnAssetDeleted callback: " + path))
                    return;

                node.parent.RemoveAt(node.Index);
                OnRefreshFinished();
            }));
        }

        private void OnAssetRenamed(string from, string to)
        {
            treeListView.InvokeIfCreated(new MethodInvoker(() =>
            {
                var node = m_AssetTree.FindNodeFromPath(from);
                if (node == null && m_AssetTree.FindNodeFromPath(to) != null)
                    return; // This means that asset was renamed via treeListView UI which directly modifies node name before sending it to backend

                if (Logger.AssertIf(node == null, "Could not find node in UI in OnAssetRenamed callback: " + from))
                    return;

                var dirPath = Path.GetDirectoryName(to);
                var parentNode = m_AssetTree.FindNodeFromPath(dirPath);
                if (Logger.AssertIf(node == null, "parentNode could not be found in UI in OnAssetRenamed callback: " + dirPath + " :: " + to))
                    return;

                node.parent.RemoveAt(node.Index);
                parentNode.AddNode(node);

                OnRefreshFinished();
            }));
        }

        #endregion

        private void OnAfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (e.Label == "" || e.Label == null)
            {
                e.CancelEdit = true;
                return;
            }

            try
            {
                var assetNode = treeListView.SelectedObject as TreeNode<Asset>;
                var asset = assetNode.value;
                var newPath = asset.Path.Replace(asset.Name + Path.GetExtension(asset.Path), e.Label + Path.GetExtension(asset.Path));
                AssetManager.RenameAsset(asset.Path, newPath);
            }
            catch
            {
                e.CancelEdit = true;
            }
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

            asset.Importer.Load<object>(); // Force loading before check
            if (asset.Importer.LoadingFailed)
            {
                Logger.Logi(LogType.Error, $"This asset failed to import: {asset.Path}, probably unknown extension or corrupted file.");
                return;
            }

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

            asset.Importer.Load<object>(); // Force loading before check
            if (asset.Importer.LoadingFailed)
                return;

            AssetSelected?.Invoke();
        }

        private void treeListView_ModelCanDrop(object sender, ModelDropEventArgs e)
        {
            var targetNode = e.TargetModel as TreeNode<Asset>;
            var sourceNode = e.SourceModels[0] as TreeNode<Asset>;

            if (targetNode == null || sourceNode == null || targetNode.value == null)
            {
                e.Effect = DragDropEffects.None;
                e.DropSink.CanDropBetween = true;
                e.DropSink.CanDropOnItem = false;
                return;
            }

            var isOriginalTargetFolder = Paths.IsDirectory(targetNode.value.Path);

            // If dropping in between some nodes, we actually want to put it inside the parent node
            var isBetween = e.DropTargetLocation == DropTargetLocation.BelowItem || e.DropTargetLocation == DropTargetLocation.AboveItem;
            var newTarget = !isBetween ? targetNode : targetNode.parent.value == null ? targetNode : targetNode.parent;

            var sourcePath = sourceNode.value.Path;
            var newTargetPath = Path.Combine(newTarget.value.Path, Path.GetFileName(sourcePath));

            var canBeDropped = sourcePath != newTargetPath && !newTargetPath.StartsWith(sourcePath) &&
                newTargetPath != sourcePath;

            e.DropSink.CanDropBetween = true;
            e.DropSink.CanDropOnItem = isOriginalTargetFolder;
            e.Effect = canBeDropped ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void treeListView_ModelDropped(object sender, ModelDropEventArgs e)
        {
            var targetNode = e.TargetModel as TreeNode<Asset>;
            var sourceNode = e.SourceModels[0] as TreeNode<Asset>;

            var isBetween = e.DropTargetLocation == DropTargetLocation.BelowItem || e.DropTargetLocation == DropTargetLocation.AboveItem;
            if (isBetween)
                targetNode = targetNode.parent;

            var sourcePath = sourceNode.value.Path;
            var targetPath = Path.Combine(targetNode.value.Path, Path.GetFileName(sourcePath));
            AssetManager.RenameAsset(sourcePath, targetPath);

            var droppedAssetNode = m_AssetTree.FindNodeFromPath(targetPath);
            if (Logger.AssertIf(droppedAssetNode == null, "Cannot select asset which was just dropped. Looks like something went wrong. Please report a bug"))
                return;

            treeListView.SelectedObject = droppedAssetNode;
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
                        GetDirectoryPathFromSelection(out TreeNode<Asset> assetNode, out bool isFile, out string dirPath);

                        var script = ScriptTemplates.GetTemplate(name);
                        var fileName = ScriptTemplates.GetTemplateFileName(name);
                        var filePath = Path.Combine(dirPath, fileName + ".cs");
                        filePath = Paths.GetUniquePath(filePath);
                        AssetManager.CreateAsset(script, filePath);

                        // Expand parent node if folder was created inside it
                        if (!isFile)
                            treeListView.Expand(assetNode);

                        // Move Selection to newly selected folder
                        var addedUiObject = m_AssetTree.FindNodeFromPath(filePath);
                        if (addedUiObject != null)
                            treeListView.SelectedObject = addedUiObject;
                        else
                            Logger.Logi(LogType.Error, "Cannot select newly created asset, something must've gone wrong in AssetsWindow");

                    };
                    menuItem.DropDownItems.Add(item);
                }
            }));
        }

        /// <summary>
        /// If folder is selected, will return folder path
        /// If file is selected, will return path of directory where that file exists
        /// If nothing is selected, will return Assets folder
        /// </summary>
        private void GetDirectoryPathFromSelection(out TreeNode<Asset> assetNode, out bool isFile, out string dirPath)
        {
            assetNode = treeListView.SelectedObject as TreeNode<Asset>;
            assetNode = assetNode == null ? m_AssetTree.GetChild(0) : assetNode;
            var asset = assetNode.value;

            isFile = File.Exists(asset.Path);
            dirPath = isFile ? Paths.GetRelativePath(Path.GetDirectoryName(asset.Path)) : asset.Path;
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
            GetDirectoryPathFromSelection(out TreeNode<Asset> assetNode, out bool _, out string _);

            var path = Path.Combine(Environment.CurrentDirectory, assetNode.value.Path);
            System.Diagnostics.Process.Start("explorer.exe", "/select, " + path);
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeListView.SelectedObject != null)
                treeListView.EditModel(treeListView.SelectedObject);
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

            // Put correct message in the message box
            var isFile = File.Exists(asset.Path);
            var msg = (isFile ? Properties.Resources.S_ConfirmAssetDeletionMessage
                : Properties.Resources.S_ConfirmFolderDeletionMessage) +
                "\n\n\t" + asset.Path;

            // Show dialog to confirm deletion
            var res = FlexibleMessageBox.Show(msg, "Confirm deletion", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (res == DialogResult.OK)
                AssetManager.DeleteAsset(asset.Path);

        }

        private void newFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetDirectoryPathFromSelection(out TreeNode<Asset> assetNode, out bool isFile, out string dirPath);

            var preferredPath = Path.Combine(dirPath, "New Folder");
            var uniqueDirPath = Paths.GetUniquePath(preferredPath);

            AssetManager.CreateAsset(null, uniqueDirPath);

            // Expand parent node if folder was created inside it
            if (!isFile)
                treeListView.Expand(assetNode);

            // Move Selection to newly selected folder
            var addedUiObject = m_AssetTree.FindNodeFromPath(uniqueDirPath);
            if (addedUiObject != null)
                treeListView.SelectedObject = addedUiObject;
            else
                Logger.Logi(LogType.Error, "Cannot select newly created folder, something must've gone wrong in AssetsWindow");
        }

        #endregion

        #region ToolStrip Buttons

        private void ToolstripExpandAll_Click(object sender, EventArgs e)
        {
            treeListView.ExpandAll();
        }

        private void ToolstripExpandOne_Click(object sender, EventArgs e)
        {
            treeListView.CollapseAll();
            treeListView.Expand(m_AssetTree.GetChild(0));
            foreach (var node in m_AssetTree.GetChild(0))
                treeListView.Expand(node);
        }

        private void ToolstripCollapseAll_Click(object sender, EventArgs e)
        {
            treeListView.CollapseAll();
            treeListView.Expand(m_AssetTree.GetChild(0));
        }

        #endregion

        private void ASSERT_TreeViewIsTheSameAsInRecordingManager()
        {
#if ENABLE_UI_TESTING
            // On first refresh asset tree is empty since it was not initialized and callbacks earlied out
            // But AssetManager will have all the assets, so this method will print lots of errors
            // Skipping for first time
            if (m_AssetTree.Count() == 0)
                return;

            foreach (var asset in AssetManager.Assets)
            {
                var node = m_AssetTree.FindNodeFromPath(asset.Path);
                var assetFound = node != null && node.value is Asset value && value.Path == asset.Path;

                Logger.AssertIf(!assetFound, $"Asset was not found in AssetsWindow, but exists in AssetManager: {asset.Path}");
            }
#endif
        }
    }
}
