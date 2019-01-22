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
            treeView.Font = Fonts.Default;
            treeView.NodeMouseClick += (sender, args) => treeView.SelectedNode = args.Node;

            treeView.AfterRename += OnAfterRenameNode;
            treeView.HandleCreated += OnRefreshFinished;

            contextMenuStrip.HandleCreated += (s, e) => AddMenuItemsForScriptTemplates(contextMenuStrip, "addScriptToolStripMenuItem");
        }

        private void OnRefreshFinished(object sender, EventArgs e)
        {

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
