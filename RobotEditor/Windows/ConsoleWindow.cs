using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using RobotRuntime;
using BrightIdeasSoftware;
using RobotRuntime.Abstractions;
using RobotRuntime.Logging;
using RobotEditor.Abstractions;
using System.Collections;

namespace RobotEditor.Windows
{
    public partial class ConsoleWindow : DockContent, IConsoleWindow
    {
        private ILogger Logger;
        public ConsoleWindow(ILogger Logger)
        {
            this.Logger = Logger;

            InitializeComponent();
            AddToolstripButtons();
            AutoScaleMode = AutoScaleMode.Dpi;

            treeListView.Font = Fonts.Default;

            Logger.OnLogReceived += OnLogReceived;
            Logger.LogCleared += OnLogCleared;

            CreateColumns();
            UpdateHierarchy();
        }

        private void AddToolstripButtons()
        {
            var snapshotButton = new ToolStripButton("Clear");
            snapshotButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            snapshotButton.Click += ClearLog;
            toolStrip.Items.Add(snapshotButton);
        }

        private void ClearLog(object sender, EventArgs e)
        {
            Logger.Clear();
        }

        private void UpdateHierarchy()
        {
            treeListView.Roots = Logger.LogList;
            treeListView.Refresh();
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
            treeListView.CanExpandGetter = x => ((Log)x).Description != null;
            treeListView.ChildrenGetter = x => GenerateDescriptionItemsFromLog(((Log)x));

            var typeColumn = new OLVColumn("Type", "Type");
            typeColumn.AspectGetter = x =>
            {
                var logType = ((Log)x).LogType;
                return logType != LogType.None ? logType.ToString() : "";
            };

            var nameColumn = new OLVColumn("Name", "Name");
            nameColumn.AspectGetter = x => ((Log)x).Header;

            /*var stackColumn = new OLVColumn("Stacktrace", "Stacktrace");
            stackColumn.AspectGetter = x => ((Log)x).Stacktrace;
            stackColumn.WordWrap = true;*/

            treeListView.UseCellFormatEvents = true;
            treeListView.FormatCell += UpdateFontsTreeListView;

            treeListView.FullRowSelect = true;


            treeListView.Columns.Add(typeColumn);
            treeListView.Columns.Add(nameColumn);
            //treeListView.Columns.Add(stackColumn);
        }

        private IEnumerable GenerateDescriptionItemsFromLog(Log x)
        {
            var sentences = x.Description.Split('\n');
            foreach(var str in sentences)
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
            treeListView.Columns[0].Width = (int)(treeListView.Width * 0.10f);
            treeListView.Columns[1].Width = (int)(treeListView.Width * 0.88f);
        }

        private void showStacktraceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var obj = treeListView.SelectedObject;
            if (obj == null)
                return;

            var log = (Log)obj;
            if (log.IsDefault())
                return;

            if (log.Stacktrace != null)
                FlexibleMessageBox.Show(log.Header + "\n\n" + log.Stacktrace, "Stacktrace", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
