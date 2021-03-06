﻿#define ENABLE_UI_TESTING

using BrightIdeasSoftware;
using Robot;
using Robot.Abstractions;
using Robot.Assets;
using RobotEditor.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Assets;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Unity.Lifetime;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor
{
    [RegisterTypeToContainer(typeof(IAssetsWindow), typeof(ContainerControlledLifetimeManager))]
    public partial class AssetsWindow : DockContent, IAssetsWindow
    {
        public event Action AssetSelected;

        public ToolStrip ToolStrip { get { return toolStrip; } }

        private bool m_SupressRefreshAndSelection = false;

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

            AssetManager.RefreshFinished += () => OnRefreshFinished();
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
                if (!node.value.LoadingFailed)
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

        private void treeListView_Resize(object sender, EventArgs e)
        {
            treeListView.Columns[0].Width = (int)(treeListView.Width * 0.98f);
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
                    var assetPath = asset.Path;
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

        private void OnRefreshFinished(Action afterRefreshCallback = null)
        {
            if (m_SupressRefreshAndSelection)
                return;

            treeListView.InvokeIfCreated(new MethodInvoker(() =>
            {
                treeListView.Roots = m_AssetTree;
                treeListView.Sort(0);
                treeListView.Refresh();
                treeListView.Expand(m_AssetTree.GetChild(0));

                afterRefreshCallback?.Invoke();

                ASSERT_TreeViewIsTheSameAsInRecordingManager();
            }));
        }

        private void OnAssetCreated(string path)
        {
            treeListView.InvokeIfCreated(new MethodInvoker(() =>
            {
                var asset = AssetManager.GetAsset(path);
                var assetPath = asset.Path;
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

                // Do not refresh while many assets are being edited, it will fail safety DEBUG checks and slow everything down
                if (!AssetManager.IsEditingAssets)
                    OnRefreshFinished();
            }));
        }

        private void OnAssetDeleted(string path)
        {
            treeListView.InvokeIfCreated(new MethodInvoker(() =>
            {
                var node = m_AssetTree.FindNodeFromPath(path);
                if (node == null) // Might be that asset was deleted together with folder (upon refresh). Both callbacks will be called
                    return;

                node.parent.RemoveAt(node.Index);

                // Do not refresh while many assets are being edited, it will fail safety DEBUG checks and slow everything down
                if (!AssetManager.IsEditingAssets)
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

                var dirPath = Path.GetDirectoryName(to);
                var parentNode = m_AssetTree.FindNodeFromPath(dirPath);
                if (parentNode == null)
                    return; // parentNode could not be found in UI in OnAssetRenamed callback. This happens when renaming folder together with assets inside

                if (Logger.AssertIf(node == null, "Could not find node in UI in OnAssetRenamed callback: " + from))
                    return;

                node.parent.RemoveAt(node.Index);
                parentNode.AddNode(node);

                // Do not refresh while many assets are being edited, it will fail safety DEBUG checks and slow everything down
                if (!AssetManager.IsEditingAssets)
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

            if (asset.LoadingFailed)
            {
                Logger.Logi(LogType.Error, $"This asset failed to import: {asset.Path}, probably unknown extension or corrupted file.");
                return;
            }

            // Iterating all asset importer types, each importer does something specific when double clicked
            if (asset.ImporterType == typeof(DirectoryImporter))
            {
                var isExpanded = treeListView.IsExpanded(treeListView.SelectedObject);
                if (isExpanded)
                    treeListView.Collapse(treeListView.SelectedObject);
                else
                    treeListView.Expand(treeListView.SelectedObject);
            }
            else if (asset.HoldsTypeOf(typeof(Recording)))
            {
                if (!RecordingManager.LoadedRecordings.Any(s => s.Name == asset.Name))
                    RecordingManager.LoadRecording(asset.Path);
            }
            else if (asset.HoldsTypeOf(typeof(LightTestFixture)))
            {
                LoadTestFixtureFromAsset(asset);
                // TODO: Send some message to main form to give focus to window is TestFixture is already open
            }
            else if (asset.ImporterType == typeof(ScriptImporter))
            {
                Task.Run(() => CodeEditor.FocusFile(asset.Path));
            }
        }

        private void treeListView_SelectionChanged(object sender, EventArgs e)
        {
            var assetNode = treeListView.SelectedObject as TreeNode<Asset>;
            var asset = assetNode?.value;
            if (asset == null)
                return;

            if (asset.LoadingFailed)
                return;

            AssetSelected?.Invoke();
        }

        private void treeListView_ModelCanDrop(object sender, ModelDropEventArgs e)
        {
            var sourceNodes = e.SourceModels.SafeCast<TreeNode<Asset>>();
            e.DropSink.CanDropBetween = true;

            if (!(e.TargetModel is TreeNode<Asset> targetNode))
            {
                e.Effect = DragDropEffects.None;
                e.DropSink.CanDropOnItem = false;
                return;
            }

            e.DropSink.CanDropOnItem = Paths.IsDirectory(targetNode.value.Path);

            // If dropping in between some nodes, we actually want to put it inside the parent node if one exist
            var isBetween = e.DropTargetLocation == DropTargetLocation.BelowItem || e.DropTargetLocation == DropTargetLocation.AboveItem;
            var newTarget = !isBetween ? targetNode : targetNode.parent?.value == null ? targetNode : targetNode.parent;

            // If dragged object contains nulls, cancel
            if (sourceNodes.Any(n => n == null || n.value == null))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            var nodePaths = sourceNodes.Select(n => (From: n.value.Path.NormalizePath(),
                To: Path.Combine(newTarget.value.Path, Path.GetFileName(n.value.Path)).NormalizePath()));

            // if paht to starts with path from AND they are not the same, it is being nested under itself
            var areNodesBeingNestedUnderThemself = nodePaths.Any(p => p.From != p.To && p.To.IsSubDirectoryOf(p.From));
            var areAllNodesBeingMovedToSamePath = nodePaths.All(p => p.From == p.To);

            var canBeDropped = !areAllNodesBeingMovedToSamePath && !areNodesBeingNestedUnderThemself;

            e.Effect = canBeDropped ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void treeListView_ModelDropped(object sender, ModelDropEventArgs e)
        {
            var targetNode = e.TargetModel as TreeNode<Asset>;
            var sourceNodes = e.SourceModels.SafeCast<TreeNode<Asset>>();
            var allSelectedNodes = sourceNodes.Select(n => n.value.Path);

            var isBetween = e.DropTargetLocation == DropTargetLocation.BelowItem || e.DropTargetLocation == DropTargetLocation.AboveItem;
            if (isBetween)
                targetNode = targetNode.parent;

            // Caching nodes prior filtering, so we can correctly select after drag is completed
            var cachedDraggedNodesForSelection = sourceNodes.Select(n => (From: n.value.Path.NormalizePath(),
                To: Path.Combine(targetNode.value.Path, Path.GetFileName(n.value.Path)).NormalizePath()));

            // Do not move nodes if already moving node parent
            var filteredNodes = RemoveChildNodesIfParentIsAlsoDragged(sourceNodes);

            var nodePaths = filteredNodes.Select(n => (From: n.value.Path.NormalizePath(),
                To: Path.Combine(targetNode.value.Path, Path.GetFileName(n.value.Path)).NormalizePath()));

            // Do not move nodes if it's not needed
            nodePaths = nodePaths.Where(p => p.From != p.To);

            // Deselecting all while drag and drop finishes
            treeListView.SelectedObjects = null;
            treeListView.Refresh();

            // Renaming actual assets in backend. UI will be updated by callbacks and new nodes with new paths will be created in list view
            // Beginning editing will not allow refresh to be called in the middle of file moving
            // This might cause problems if compilation starts while half of the assets are still being moved
            m_SupressRefreshAndSelection = true;
            AssetManager.BeginAssetEditing();
            foreach (var (From, To) in nodePaths)
                AssetManager.RenameAsset(From, To);
            AssetManager.EndAssetEditing();
            m_SupressRefreshAndSelection = false;

            treeListView.Focus();

            // Selecting newly dropped nodes
            var nodesToSelect = cachedDraggedNodesForSelection.Select(p => m_AssetTree.FindNodeFromPath(p.To)).Where(n => n != null);
            OnRefreshFinished(() => treeListView.SelectedObjects = nodesToSelect.ToList());
        }

        private IEnumerable<TreeNode<Asset>> RemoveChildNodesIfParentIsAlsoDragged(IEnumerable<TreeNode<Asset>> sourceNodes)
        {
            var nodesToExclude = (from s in sourceNodes
                                  from n in sourceNodes
                                  where s.Contains(n)
                                  select n);

            return sourceNodes.Except(nodesToExclude);
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

        private void reloadFixtureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var assetNode = treeListView.SelectedObject as TreeNode<Asset>;
            var asset = assetNode?.value;
            LoadTestFixtureFromAsset(asset, true);
        }

        private void LoadTestFixtureFromAsset(Asset asset, bool forceReloadIfAlreadyLoaded = false)
        {
            if (asset == null || !asset.HoldsTypeOf(typeof(LightTestFixture)))
                return;

            LightTestFixture fix = null;
            if (!TestFixtureManager.Contains(asset.Name))
            {
                fix = asset.ReloadAsset<LightTestFixture>();
                if (fix != null)
                {
                    fix.Name = asset.Name;
                    var fixture = TestFixtureManager.NewTestFixture(fix);
                    fixture.Path = asset.Path;
                }
            }

            else if (forceReloadIfAlreadyLoaded)
            {
                fix = asset.ReloadAsset<LightTestFixture>();
                if (fix != null)
                {
                    fix.Name = asset.Name;
                    var fixture = TestFixtureManager.Fixtures.First(f => f.Name == asset.Name);
                    fixture.ApplyLightFixtureValues(fix);
                }
            }
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
            var sourceNodes = treeListView.SelectedObjects.SafeCast<TreeNode<Asset>>();
            if (sourceNodes.Any(n => n == null || n.value == null))
            {
                Logger.Logi(LogType.Warning, "Some nodes are null or contain null value. Aborting deletions for safety reasons");
                return;
            }

            // Prepare correct warning message for user to confirm deletion
            var msg = "";
            if (sourceNodes.Count() == 1)
            {
                var isFile = File.Exists(sourceNodes.First().value.Path);
                msg = (isFile ? Properties.Resources.S_ConfirmAssetDeletionMessage
                    : Properties.Resources.S_ConfirmFolderDeletionMessage) +
                    "\n\n\t" + sourceNodes.First().value.Path;
            }
            else
            {
                var messageContent = string.Join(Environment.NewLine, sourceNodes.Select(n => "\t" + n.value.Path));
                msg = Properties.Resources.S_ConfirmMultipleAssetDeletionMessage + "\n\n" + messageContent;
            }

            // Show dialog to confirm deletion
            var res = FlexibleMessageBox.Show(msg, "Confirm deletion", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (res == DialogResult.OK)
            {
                var filteredNodes = RemoveChildNodesIfParentIsAlsoDragged(sourceNodes);

                treeListView.SelectedObjects = null;
                AssetManager.BeginAssetEditing();
                foreach (var n in filteredNodes)
                    AssetManager.DeleteAsset(n.value.Path);
                AssetManager.EndAssetEditing();

                OnRefreshFinished(() => treeListView.SelectedObjects = null);
            }
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

        [Conditional("DEBUG")]
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
