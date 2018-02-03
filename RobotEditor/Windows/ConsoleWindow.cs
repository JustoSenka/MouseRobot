using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using RobotRuntime;
using BrightIdeasSoftware;
using RobotRuntime.Abstractions;
using RobotRuntime.Logging;
using RobotEditor.Abstractions;
using System.Collections;
using RobotEditor.Properties;
using RobotEditor.CustomControls;
using System.Linq;

namespace RobotEditor.Windows
{
    public partial class ConsoleWindow : DockContent, IConsoleWindow
    {
        private bool m_ControlCreated;
        private object m_CachedSelectedObject;

        private ToolStripToggleButton m_ErrorFilter;
        private ToolStripToggleButton m_WarningFilter;
        private ToolStripToggleButton m_InfoFilter;
        private ToolStripToggleButton m_DebugFilter;

        private ILogger Logger;
        public ConsoleWindow(ILogger Logger)
        {
            this.Logger = Logger;

            InitializeComponent();
            AddToolstripButtons();
            AutoScaleMode = AutoScaleMode.Dpi;

            AddTestLogs(Logger);

            treeListView.Font = Fonts.Default;

            Logger.OnLogReceived += OnLogReceived;
            Logger.LogCleared += OnLogCleared;

            this.Shown += OnFormShown;
            CreateColumns();
        }

        private static void AddTestLogs(ILogger Logger)
        {
            Logger.Logi(LogType.Debug, "1 some debug", "somethibng\nsomethibng");
            Logger.Logi(LogType.Error, "2 some error");
            Logger.Logi(LogType.Log, "3 log", "somethibng\nsomethibng");
            Logger.Logi(LogType.Warning, "4 warning type", "somethibng\nsomethibng");
            Logger.Logi(LogType.Log, "5 log", "somethibng\nsomethibng");
            Logger.Logi(LogType.Log, "6 log", "somethibng\nsomethibng");
            Logger.Logi(LogType.Log, "7 log", "somethibng\nsomethibng");
            Logger.Logi(LogType.Log, "8 log");
            Logger.Logi(LogType.Warning, "9 warning type", "somethibng\nsomethibng");
            Logger.Logi(LogType.Warning, "10 warning type", "somethibng\nsomethibng");
            Logger.Logi(LogType.Error, "11 some error");
        }

        private void AddToolstripButtons()
        {
            var clearButton = new ToolStripButton("Clear");
            clearButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            clearButton.Click += ClearLog;
            clearButton.Padding = new Padding(10, 0, 10, 0);
            clearButton.Margin = new Padding(0, 0, 10, 0);
            toolStrip.Items.Add(clearButton);

            m_ErrorFilter = new ToolStripToggleButton("0");
            m_ErrorFilter.TextImageRelation = TextImageRelation.ImageBeforeText;
            m_ErrorFilter.Image = Resources.LogError_32;
            m_ErrorFilter.ImageActive = Resources.LogError_32;
            m_ErrorFilter.ImageNotActive = Resources.LogError_d_32;
            m_ErrorFilter.ActiveStateChanged += UpdateHierarchy;
            m_ErrorFilter.Padding = new Padding(10, 0, 10, 0);
            toolStrip.Items.Add(m_ErrorFilter);

            m_WarningFilter = new ToolStripToggleButton("0");
            m_WarningFilter.TextImageRelation = TextImageRelation.ImageBeforeText;
            m_WarningFilter.Image = Resources.LogWarning_32;
            m_WarningFilter.ImageActive = Resources.LogWarning_32;
            m_WarningFilter.ImageNotActive = Resources.LogWarning_d_32;
            m_WarningFilter.ActiveStateChanged += UpdateHierarchy;
            m_WarningFilter.Padding = new Padding(10, 0, 10, 0);
            toolStrip.Items.Add(m_WarningFilter);

            m_InfoFilter = new ToolStripToggleButton("0");
            m_InfoFilter.TextImageRelation = TextImageRelation.ImageBeforeText;
            m_InfoFilter.Image = Resources.LogInfo_32;
            m_InfoFilter.ImageActive = Resources.LogInfo_32;
            m_InfoFilter.ImageNotActive = Resources.LogInfo_d_32;
            m_InfoFilter.ActiveStateChanged += UpdateHierarchy;
            m_InfoFilter.Padding = new Padding(10, 0, 10, 0);
            toolStrip.Items.Add(m_InfoFilter);

            m_DebugFilter = new ToolStripToggleButton("0");
            m_DebugFilter.TextImageRelation = TextImageRelation.ImageBeforeText;
            m_DebugFilter.Image = Resources.LogDebug_32;
            m_DebugFilter.ImageActive = Resources.LogDebug_32;
            m_DebugFilter.ImageNotActive = Resources.LogDebug_d_32;
            m_DebugFilter.ActiveStateChanged += UpdateHierarchy;
            m_DebugFilter.Padding = new Padding(10, 0, 10, 0);
            toolStrip.Items.Add(m_DebugFilter);
        }

        private void ClearLog(object sender, EventArgs e)
        {
            Logger.Clear();
        }

        private void UpdateHierarchy()
        {
            if (!m_ControlCreated)
                return;

            this.BeginInvoke(new MethodInvoker(delegate
            {
                var activeLogTypeBits = LogType.None;
                activeLogTypeBits = (m_InfoFilter.Active) ? activeLogTypeBits | LogType.Log : activeLogTypeBits;
                activeLogTypeBits = (m_ErrorFilter.Active) ? activeLogTypeBits | LogType.Error : activeLogTypeBits;
                activeLogTypeBits = (m_WarningFilter.Active) ? activeLogTypeBits | LogType.Warning : activeLogTypeBits;
                activeLogTypeBits = (m_DebugFilter.Active) ? activeLogTypeBits | LogType.Debug : activeLogTypeBits;

                treeListView.Roots = Logger.LogList.Where(log => activeLogTypeBits.HasFlag(log.LogType));
                treeListView.Refresh();

                m_ErrorFilter.Text = Logger.LogList.Count(log => log.LogType == LogType.Error) + "";
                m_WarningFilter.Text = Logger.LogList.Count(log => log.LogType == LogType.Warning) + "";
                m_InfoFilter.Text = Logger.LogList.Count(log => log.LogType == LogType.Log) + "";
                m_DebugFilter.Text = Logger.LogList.Count(log => log.LogType == LogType.Debug) + "";
            }));
        }

        private void OnLogReceived(Log obj)
        {
            UpdateHierarchy();
        }

        private void OnLogCleared()
        {
            UpdateHierarchy();
        }

        private void CreateColumns()
        {
            treeListView.CanExpandGetter = x => ((Log)x).HasDescription();
            treeListView.ChildrenGetter = x => GenerateDescriptionItemsFromLog(((Log)x));

            var imageColumn = new OLVColumn("", "Image");
            imageColumn.ImageGetter += x =>
            {
                var logType = ((Log)x).LogType;
                var index = logType == LogType.Log ? 0 : logType == LogType.Warning ? 1 : logType == LogType.Error ? 2 : logType == LogType.Debug ? 3 : -1;

                return index;
            };

            var typeColumn = new OLVColumn("Type", "Type");
            typeColumn.AspectGetter = x =>
            {
                var logType = ((Log)x).LogType;
                return logType != LogType.None ? logType.ToString() : "";
            };

            var nameColumn = new OLVColumn("Name", "Name");
            nameColumn.AspectGetter = x =>
            {
                var log = ((Log)x);
                return log.HasDescription() ? log.Header + " ..." : log.Header;
            };

            treeListView.UseCellFormatEvents = true;
            treeListView.FormatCell += UpdateFontsTreeListView;
            treeListView.CustomSorter = (x, y) => { };

            treeListView.FullRowSelect = true;
            treeListView.TreeColumnRenderer.IsShowLines = false;
            treeListView.TreeColumnRenderer.IsShowGlyphs = false;

            treeListView.Columns.Add(imageColumn);
            treeListView.Columns.Add(typeColumn);
            treeListView.Columns.Add(nameColumn);
        }

        private IEnumerable GenerateDescriptionItemsFromLog(Log x)
        {
            var sentences = x.Description.Split('\n');
            foreach (var str in sentences)
            {
                yield return new Log(LogType.None, str, "", null);
            }
        }

        private void UpdateFontsTreeListView(object sender, FormatCellEventArgs e)
        {

        }

        public ToolStrip ToolStrip { get { return toolStrip; } }

        private void treeListView_Resize(object sender, EventArgs e)
        {
            treeListView.Columns[0].Width = 50;
            treeListView.Columns[1].Width = 70;
            treeListView.Columns[2].Width = (int)(treeListView.Width * 0.98f) - 120;
        }

        private void showStacktraceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var obj = treeListView.SelectedObject;
            if (obj == null)
                return;

            var log = (Log)obj;
            if (log.IsDefault())
                return;

            var descAddition = log.HasDescription() ? log.Description + "\n\n" : "";
            if (log.Stacktrace != null)
                FlexibleMessageBox.Show(log.Header + "\n\n" + descAddition + log.Stacktrace, "Stacktrace", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void treeListView_SelectionChanged(object sender, EventArgs e)
        {
            if (treeListView.SelectedObject == null)
                return;

            if (m_CachedSelectedObject == treeListView.SelectedObject)
                return;

            var log = (Log)treeListView.SelectedObject;

            if (log.IsDefault() || log.LogType == LogType.None)
                return;

            m_CachedSelectedObject = treeListView.SelectedObject;
            treeListView.CollapseAll();
            treeListView.Expand(log);
        }

        private void OnFormShown(object sender, EventArgs e)
        {
            m_ControlCreated = true;
            UpdateHierarchy();
        }
    }
}
