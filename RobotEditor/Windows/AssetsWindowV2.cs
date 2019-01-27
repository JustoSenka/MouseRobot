using BrightIdeasSoftware;
using Robot;
using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                //imageListIndex = node.Recording != null ? 0 : imageListIndex;
                //imageListIndex = node.Command != null ? 1 : imageListIndex;
                return imageListIndex;
            };

            treeListView.UseCellFormatEvents = true;

            treeListView.IsSimpleDragSource = true;
            treeListView.IsSimpleDropSink = true;

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
                        if (c == null) // Only add if asset or directory does not exist
                        {
                            var intermediateAsset = AssetManager.GetAsset(path);
                            m_AssetTree.AddChildAtPath(path, intermediateAsset);
                        }
                    }
                }

                treeListView.Roots = m_AssetTree;

                /*
                for (int i = 0; i < treeListView.Items.Count; ++i)
                    treeListView.Items[i].ImageIndex = 0;*/

                treeListView.Refresh();
            }));
        }

        private void treeListView_SelectionChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void treeListView_ModelDropped(object sender, ModelDropEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void treeListView_ModelCanDrop(object sender, ModelDropEventArgs e)
        {
            throw new NotImplementedException();
        }


        private void OnAfterRenameNode(NodeLabelEditEventArgs obj)
        {

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
            return null;
        }

        #region Context Menu Items

        private void reloadRecordingToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AssetManager.Refresh();
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {

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

        #endregion
    }
}
