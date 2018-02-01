using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using RobotRuntime;
using BrightIdeasSoftware;
using RobotRuntime.Abstractions;
using RobotRuntime.Logging;
using RobotEditor.Abstractions;
using System.Collections;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace RobotEditor.Windows
{
    public partial class ConsoleWindow : DockContent, IConsoleWindow
    {
        private bool m_ControlCreated;
        private object m_CachedSelectedObject;

        private ILogger Logger;
        public ConsoleWindow(ILogger Logger)
        {
            this.Logger = Logger;

            InitializeComponent();
            AddToolstripButtons();
            AddIconsToImageList();
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
            Logger.Logi(LogType.Debug, "some debug", "somethibng\nsomethibng");
            Logger.Logi(LogType.Error, "some error");
            Logger.Logi(LogType.Log, "log", "somethibng\nsomethibng");
            Logger.Logi(LogType.Warning, "warning type", "somethibng\nsomethibng");
            Logger.Logi(LogType.Log, "log", "somethibng\nsomethibng");
            Logger.Logi(LogType.Warning, "warning type", "somethibng\nsomethibng");
            Logger.Logi(LogType.Error, "some error"); 
        }

        private void AddToolstripButtons()
        {
            var snapshotButton = new ToolStripButton("Clear");
            snapshotButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            snapshotButton.Click += ClearLog;
            toolStrip.Items.Add(snapshotButton);
        }

        private void AddIconsToImageList()
        {
            imageList.Images.Add(SystemIcons.Information);
            imageList.Images.Add(SystemIcons.Warning);
            imageList.Images.Add(SystemIcons.Error);
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
                treeListView.Roots = Logger.LogList;
                treeListView.Refresh();
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
            imageColumn.ImageGetter += delegate (object x)
            {
                var index = (int)((Log)x).LogType - 1;
                if (index > 2)
                    index = 0;

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

            treeListView.FullRowSelect = true;
            treeListView.TreeColumnRenderer.IsShowLines = false;
            treeListView.TreeColumnRenderer.IsShowGlyphs = false;

            treeListView.ShowFilterMenuOnRightClick = true; // ContextMenu is overriden now, so no work

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
