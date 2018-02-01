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

namespace RobotEditor.Windows
{
    public partial class ConsoleWindow : DockContent, IConsoleWindow
    {
        private Tuple<Log, IList<string>> m_CachedLogEntries = new Tuple<Log, IList<string>>(default(Log), null);
        
        private ILogger Logger;
        public ConsoleWindow(ILogger Logger)
        {
            this.Logger = Logger;

            InitializeComponent();
            AddToolstripButtons();
            AddIconsToImageList();
            AutoScaleMode = AutoScaleMode.Dpi;

            Logger.Logi(LogType.Debug, "some debug", "somethibng\nsomethibng");
            Logger.Logi(LogType.Error, "some error");
            Logger.Logi(LogType.Log, "log", "somethibng\nsomethibng");
            Logger.Logi(LogType.Warning, "warning type", "somethibng\nsomethibng");

            treeListView.Font = Fonts.Default;

            Logger.OnLogReceived += OnLogReceived;
            Logger.LogCleared += OnLogCleared;

            CreateColumns();
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
            nameColumn.AspectGetter = x => ((Log)x).Header;

            treeListView.UseCellFormatEvents = true;
            treeListView.FormatCell += UpdateFontsTreeListView;

            treeListView.FullRowSelect = true;

            treeListView.Columns.Add(imageColumn);
            treeListView.Columns.Add(typeColumn);
            treeListView.Columns.Add(nameColumn);
            //treeListView.Columns.Add(stackColumn);
        }

        private IEnumerable GetCachedOrGenerateDescriptionItemsFromLog(Log x)
        {
            throw new NotImplementedException();
            /*var cachedLog = m_CachedLogEntries.Item1;
            if (cachedLog.IsDefault())*/
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
            treeListView.Columns[0].Width = (int)(treeListView.Width * 0.03f);
            treeListView.Columns[1].Width = (int)(treeListView.Width * 0.04f);
            treeListView.Columns[2].Width = (int)(treeListView.Width * 0.91f);
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

            var log = (Log)treeListView.SelectedObject;

            if (log.IsDefault() || log.LogType == LogType.None)
                return;

            treeListView.CollapseAll();
            treeListView.Expand(log);
        }

        private void ConsoleWindow_Activated(object sender, EventArgs e)
        {
            UpdateHierarchy();
        }
    }
}
