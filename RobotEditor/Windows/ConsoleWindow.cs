using BrightIdeasSoftware;
using RobotEditor.Abstractions;
using RobotEditor.CustomControls;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Logging;
using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using Unity.Lifetime;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor.Windows
{
    [RegisterTypeToContainer(typeof(IConsoleWindow), typeof(ContainerControlledLifetimeManager))]
    public partial class ConsoleWindow : DockContent, IConsoleWindow
    {
        private bool m_ControlCreated;
        private object m_CachedSelectedObject;

        private bool m_IsUpdatingUI = false;
        private bool m_NewLogsAppearedSinceLastUpdate = false;

        private ToolStripToggleButton m_ErrorFilter;
        private ToolStripToggleButton m_WarningFilter;
        private ToolStripToggleButton m_InfoFilter;
        private ToolStripToggleButton m_DebugFilter;

        private ILogger Logger;
        private IProfiler Profiler;
        public ConsoleWindow(ILogger Logger, IProfiler Profiler)
        {
            this.Logger = Logger;
            this.Profiler = Profiler;

            InitializeComponent();
            AddToolstripButtons();
            AutoScaleMode = AutoScaleMode.Dpi;

            treeListView.Font = Fonts.Default;

            Logger.OnLogReceived += OnLogReceived;
            Logger.LogCleared += OnLogCleared;

            this.Shown += OnFormShown;
            CreateColumns();
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
            m_ErrorFilter.Image = Properties.Resources.LogError_32;
            m_ErrorFilter.ImageActive = Properties.Resources.LogError_32;
            m_ErrorFilter.ImageNotActive = Properties.Resources.LogError_d_32;
            m_ErrorFilter.ActiveStateChanged += UpdateHierarchy;
            m_ErrorFilter.Padding = new Padding(10, 0, 10, 0);
            toolStrip.Items.Add(m_ErrorFilter);

            m_WarningFilter = new ToolStripToggleButton("0");
            m_WarningFilter.TextImageRelation = TextImageRelation.ImageBeforeText;
            m_WarningFilter.Image = Properties.Resources.LogWarning_32;
            m_WarningFilter.ImageActive = Properties.Resources.LogWarning_32;
            m_WarningFilter.ImageNotActive = Properties.Resources.LogWarning_d_32;
            m_WarningFilter.ActiveStateChanged += UpdateHierarchy;
            m_WarningFilter.Padding = new Padding(10, 0, 10, 0);
            toolStrip.Items.Add(m_WarningFilter);

            m_InfoFilter = new ToolStripToggleButton("0");
            m_InfoFilter.TextImageRelation = TextImageRelation.ImageBeforeText;
            m_InfoFilter.Image = Properties.Resources.LogInfo_32;
            m_InfoFilter.ImageActive = Properties.Resources.LogInfo_32;
            m_InfoFilter.ImageNotActive = Properties.Resources.LogInfo_d_32;
            m_InfoFilter.ActiveStateChanged += UpdateHierarchy;
            m_InfoFilter.Padding = new Padding(10, 0, 10, 0);
            toolStrip.Items.Add(m_InfoFilter);

            m_DebugFilter = new ToolStripToggleButton("0");
            m_DebugFilter.TextImageRelation = TextImageRelation.ImageBeforeText;
            m_DebugFilter.Image = Properties.Resources.LogDebug_32;
            m_DebugFilter.ImageActive = Properties.Resources.LogDebug_32;
            m_DebugFilter.ImageNotActive = Properties.Resources.LogDebug_d_32;
            m_DebugFilter.ActiveStateChanged += UpdateHierarchy;
            m_DebugFilter.Padding = new Padding(10, 0, 10, 0);
            m_DebugFilter.Active = false;
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

            // When a lot of logs are printed, we update the list view after every single log
            if (m_IsUpdatingUI)
            {
                m_NewLogsAppearedSinceLastUpdate = true;
                return;
            }

            m_IsUpdatingUI = true;
            var res = this.BeginInvokeIfCreated(new MethodInvoker(delegate
            {
                Profiler.Start("ConsoleWindow_UpdateHierarchy");
                var activeLogTypeBits = LogType.None;
                activeLogTypeBits = (m_InfoFilter.Active) ? activeLogTypeBits | LogType.Log : activeLogTypeBits;
                activeLogTypeBits = (m_ErrorFilter.Active) ? activeLogTypeBits | LogType.Error : activeLogTypeBits;
                activeLogTypeBits = (m_WarningFilter.Active) ? activeLogTypeBits | LogType.Warning : activeLogTypeBits;
                activeLogTypeBits = (m_DebugFilter.Active) ? activeLogTypeBits | LogType.Debug : activeLogTypeBits;

                treeListView.Roots = Logger.LogList.Where(log => activeLogTypeBits.HasFlag(log.LogType));
                treeListView.Refresh();

                var (Error, Warning, Info, Debug) = GetCountForAllLogTypes();
                m_ErrorFilter.Text = Error + "";
                m_WarningFilter.Text = Warning + "";
                m_DebugFilter.Text = Debug + "";
                m_InfoFilter.Text = Info + "";

                m_IsUpdatingUI = false;
                Profiler.Stop("ConsoleWindow_UpdateHierarchy");

                // If new logs appeared while updating, repeat the update so we get the latest log
                if (m_NewLogsAppearedSinceLastUpdate)
                {
                    m_NewLogsAppearedSinceLastUpdate = false;
                    UpdateHierarchy();
                }
            }));

            // If control was not yet created, method invoker will do nothing so IsUpdatingUI will never be set to false again
            // We need to set it back manually so next time update is called we do not early out
            if (res == default)
                m_IsUpdatingUI = false;
        }

        /// <summary>
        /// Optmized way of counting lgos by LogType with only one iteration.
        /// The same could be achieved using Logger.LogList.Count(log => log.LogType == LogType.Error)
        /// But that would iterate the same collection 4 times (provided we have 4 different log types)
        /// </summary>
        private (int Error, int Warning, int Info, int Debug) GetCountForAllLogTypes()
        {
            (int Error, int Warning, int Info, int Debug) count = default;

            foreach (var log in Logger.LogList)
            {
                if (log.LogType == LogType.Error) ++count.Error;
                else if (log.LogType == LogType.Log) ++count.Info;
                else if (log.LogType == LogType.Debug) ++count.Debug;
                else if (log.LogType == LogType.Warning) ++count.Warning;
            }

            return count;
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
            imageColumn.AspectGetter = x => ""; // Needed to have for better performance. if not, will use reflection based aspect getter
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
