#define ENABLE_UI_TESTING

using BrightIdeasSoftware;
using Robot;
using Robot.Abstractions;
using Robot.Settings;
using RobotEditor.Abstractions;
using RobotEditor.Hierarchy;
using RobotEditor.Windows.Base;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Recordings;
using RobotRuntime.Settings;
using RobotRuntime.Utils;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Unity.Lifetime;

namespace RobotEditor
{
    [RegisterTypeToContainer(typeof(IHierarchyWindow), typeof(ContainerControlledLifetimeManager))]
    public partial class HierarchyWindow : BaseHierarchyWindow, IHierarchyWindow
    {
        private DesignSettings m_DesignSettings = new DesignSettings();

        private readonly new IHierarchyManager HierarchyManager;
        private readonly ITestRunner TestRunner;
        private readonly IAssetManager AssetManager;
        private readonly IHierarchyNodeStringConverter HierarchyNodeStringConverter;
        private readonly ISettingsManager SettingsManager;
        public HierarchyWindow(IHierarchyManager HierarchyManager, ITestRunner TestRunner, IAssetManager AssetManager,
            IHierarchyNodeStringConverter HierarchyNodeStringConverter, ICommandFactory CommandFactory, IProfiler Profiler, IAnalytics Analytics,
            ISettingsManager SettingsManager) : base(CommandFactory, Profiler, Analytics)
        {
            base.HierarchyManager = HierarchyManager;
            this.HierarchyManager = HierarchyManager;
            this.TestRunner = TestRunner;
            this.AssetManager = AssetManager;
            this.HierarchyNodeStringConverter = HierarchyNodeStringConverter;
            this.SettingsManager = SettingsManager;

            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;

            base.Name = "Hierarchy";
            base.m_TreeListView = this.treeListView;
            base.m_ToolStrip = toolStrip;
            CreateDropDetails(HierarchyManager);
            SubscribeAllEvents(HierarchyManager);

            TestRunner.TestRunEnd += OnRecordingsFinishedRunning;
            TestRunner.TestData.CommandRunningCallback += OnCommandRunning;

            // Events for methods which are in base class, since it is not possible to do so from UI
            treeListView.ModelCanDrop += treeListView_ModelCanDrop;
            treeListView.ModelDropped += treeListView_ModelDropped;
            treeListView.SelectionChanged += treeListView_SelectionChanged;
            deleteToolStripMenuItem1.Click += deleteToolStripMenuItem_Click;
            duplicateToolStripMenuItem1.Click += duplicateToolStripMenuItem1_Click;
            newRecordingToolStripMenuItem1.Click += newRecordingToolStripMenuItem1_Click;
            ToolstripExpandAll.Click += ToolstripExpandAll_Click;
            ToolstripExpandOne.Click += ToolstripExpandOne_Click;
            ToolstripCollapseAll.Click += ToolstripCollapseAll_Click;

            // subscribing for both treeListView and contextMenuStrip creation, since it's not clear which will be created first
            treeListView.HandleCreated += AddNewCommandsToCreateMenu;
            contextMenuStrip.HandleCreated += AddNewCommandsToCreateMenu;
            CommandFactory.NewUserCommands += AddNewCommandsToCreateMenu;

            treeListView.FormatCell += UpdateFontsTreeListView;
            BaseHierarchyWindow.CreateColumns(treeListView, HierarchyNodeStringConverter);

            treeListView.HandleCreated += UpdateHierarchy;

            SettingsManager.SettingsModified += _ => OnSettingsRestored();
            SettingsManager.SettingsRestored += OnSettingsRestored;
        }

        private void OnSettingsRestored()
        {
            m_DesignSettings = SettingsManager.GetSettings<DesignSettings>();
            RefreshTreeListViewAsync();
        }

        private void AddNewCommandsToCreateMenu(object sender, EventArgs e) =>
            AddNewCommandsToCreateMenu(contextMenuStrip, treeListView);

        private void AddNewCommandsToCreateMenu() =>
            AddNewCommandsToCreateMenu(contextMenuStrip, treeListView);

        private void UpdateFontsTreeListView(object sender, FormatCellEventArgs e)
        {
            var node = e.Model as HierarchyNode;
            if (node == null)
                return;

            if (node.Recording != null)
            {
                if (node.Recording == HierarchyManager.ActiveRecording && node.Recording.IsDirty)
                    e.SubItem.Font = m_DesignSettings.HierarchyRecordingFont.AppendStyle(FontStyle.Bold | FontStyle.Italic);
                else if (node.Recording == HierarchyManager.ActiveRecording)
                    e.SubItem.Font = m_DesignSettings.HierarchyRecordingFont.AppendStyle(FontStyle.Bold);
                else if (node.Recording.IsDirty)
                    e.SubItem.Font = m_DesignSettings.HierarchyRecordingFont.AppendStyle(FontStyle.Italic);
                else
                    e.SubItem.Font = m_DesignSettings.HierarchyRecordingFont;
            }

            if (node.Command != null)
            {
                if (node == m_HighlightedNode)
                    e.SubItem.BackColor = SystemColors.Highlight;

                if (node.Command.GetType() == typeof(CommandUnknown))
                    e.SubItem.ForeColor = StandardColors.Red;
            }
        }

        private void UpdateHierarchy(object sender, EventArgs args)
        {
            m_Nodes.Clear();

            foreach (var s in HierarchyManager.LoadedRecordings)
                m_Nodes.Add(new HierarchyNode(s, DropDetails));

            RefreshTreeListViewAsync(() => treeListView.ExpandAll());
        }

        private void treeListView_Resize(object sender, EventArgs e)
        {
            treeListView.Columns[0].Width = (int)(m_TreeListView.Width * 0.98f);
        }

        #region Context Menu Items

        private void setActiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedNode = treeListView.SelectedObject as HierarchyNode;
            if (selectedNode == null || selectedNode.Recording == null)
                return;

            Analytics.PushEvent(this.GetType().Name, AnalyticsEvent.A_SetActive, typeof(Recording).Name, 1);

            HierarchyManager.ActiveRecording = selectedNode.Recording;
            RefreshTreeListViewAsync();
        }

        private void treeListView_DoubleClick(object sender, EventArgs e)
        {
            setActiveToolStripMenuItem_Click(sender, e);
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedNode = treeListView.SelectedObject as HierarchyNode;
            if (selectedNode == null || selectedNode.Recording == null)
                return;

            Process.Start("explorer.exe", "/select, " + selectedNode.Recording.Path);
        }


        #endregion

        #region Menu Items (save recordings from MainForm)
        public void SaveAllRecordings()
        {
            foreach (var recording in HierarchyManager)
            {
                if (!recording.IsDirty)
                    continue;

                Analytics.PushEvent(this.GetType().Name, AnalyticsEvent.A_Save, typeof(Recording).Name, HierarchyManager.Count());

                if (recording.Path != "")
                    HierarchyManager.SaveRecording(recording, recording.Path);
                else
                    SaveSelectedRecordingWithDialog(recording, updateUI: false);
            }

            RefreshTreeListViewAsync();
        }

        public void SaveSelectedRecordingWithDialog(Recording recording, bool updateUI = true)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.InitialDirectory = Paths.AssetsPath;
            saveDialog.Filter = string.Format("Mouse Robot File (*.{0})|*.{0}", FileExtensions.Recording);
            saveDialog.Title = "Select a path for recording to save.";
            saveDialog.FileName = recording.Name + FileExtensions.RecordingD;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                Analytics.PushEvent(this.GetType().Name, AnalyticsEvent.A_Save, typeof(Recording).Name, 1);

                HierarchyManager.SaveRecording(recording, saveDialog.FileName);
                if (updateUI)
                    RefreshTreeListViewAsync();
            }
        }
        #endregion

        #region Drag & Drop

        public override bool ShouldCancelDrop(HierarchyNode targetNode, HierarchyNode sourceNode, ModelDropEventArgs e)
        {
            var cancel = base.ShouldCancelDrop(targetNode, sourceNode, e);
            return cancel ||
                targetNode.Recording == null && sourceNode.Command == null || // Cannot drag recordings onto commands
                targetNode.Recording != null && sourceNode.Recording != null && e.DropTargetLocation == DropTargetLocation.Item;// Cannot drag recordings onto recordings
        }

        #endregion

        private void TestFixtureWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnsubscribeAllEvents(HierarchyManager);

            TestRunner.TestRunEnd -= OnRecordingsFinishedRunning;
            TestRunner.TestData.CommandRunningCallback -= OnCommandRunning;
            CommandFactory.NewUserCommands -= AddNewCommandsToCreateMenu;
            treeListView.HandleCreated -= AddNewCommandsToCreateMenu;
            contextMenuStrip.HandleCreated -= AddNewCommandsToCreateMenu;
            treeListView.HandleCreated -= UpdateHierarchy;
            treeListView.FormatCell -= UpdateFontsTreeListView;
        }

        protected override void ASSERT_TreeViewIsTheSameAsInRecordingManager()
        {
#if ENABLE_UI_TESTING
            try
            {
                for (int i = 0; i < m_Nodes.Count; i++)
                {
                    Logger.Instance.AssertIf(m_Nodes[i].Recording != HierarchyManager.LoadedRecordings[i],
                        string.Format("Hierarchy recording missmatch: {0}:{1}", i, m_Nodes[i].Value.ToString()));

                    // Will not work in nested scenarios
                    for (int j = 0; j < m_Nodes[i].Recording.Commands.Count(); j++)
                    {
                        Logger.Instance.AssertIf(m_Nodes[i].Children[j].Command != HierarchyManager.LoadedRecordings[i].Commands.GetChild(j).value,
                            string.Format("Hierarchy command missmatch: {0}:{1}, {2}:{3}",
                            i, m_Nodes[i].Value.ToString(), j, m_Nodes[i].Recording.Commands.GetChild(j).value.ToString()));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Exception in ASSERT_TreeViewIsTheSameAsInRecordingManager: " + e.Message);
            }
#endif
        }
    }
}
