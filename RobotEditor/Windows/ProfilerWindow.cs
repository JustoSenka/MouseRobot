using RobotEditor.Abstractions;
using BrightIdeasSoftware;
using RobotEditor.CustomControls;
using RobotRuntime.Perf;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using RobotRuntime.Abstractions;

namespace RobotEditor.Windows
{
    public partial class ProfilerWindow : DockContent, IProfilerWindow
    {
        private Dictionary<string, ProfilerNode[]> m_NodeDict;
        private List<ProfilerNode>[] m_NodesArray;
        private List<ProfilerNode> m_Nodes;

        private const int ProfilerTimerInterval = 500;
        private Timer m_ProfilerTimer;
        private bool m_RealTimeProfilingEnabled;
        public ToolStripButton RealTimeProfilingButton { get; private set; }

        public TrackBarToolStripItem FrameSlider { get; private set; }

        private IProfiler Profiler;
        public ProfilerWindow(IProfiler Profiler)
        {
            this.Profiler = Profiler;

            InitializeComponent();
            CreateNodeList();
            CreateColumns();
            AddToolstripButtons();
            ProfilerWindow_Resize(this, null);
            OnRealTimeProfilingButton(this, null);
        }

        private void CreateNodeList()
        {
            m_Nodes = new List<ProfilerNode>();
            m_NodesArray = new List<ProfilerNode>[Profiler.NodeLimit];
        }

        private void AddToolstripButtons()
        {
            var snapshotButton = new ToolStripButton("Take Snapshot");
            snapshotButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            snapshotButton.Click += TakeSnapshot;
            toolStrip.Items.Add(snapshotButton);

            RealTimeProfilingButton = new ToolStripButton("Enable Auto Update");
            RealTimeProfilingButton.TextImageRelation = TextImageRelation.ImageBeforeText;
            RealTimeProfilingButton.Click += OnRealTimeProfilingButton;
            toolStrip.Items.Add(RealTimeProfilingButton);

            FrameSlider = new TrackBarToolStripItem();
            FrameSlider.BackColor = toolStrip.BackColor;
            FrameSlider.Height = toolStrip.Height - 5;
            FrameSlider.TrackBar.TickStyle = TickStyle.None;
            FrameSlider.TrackBar.Minimum = -Profiler.NodeLimit + 1;
            FrameSlider.TrackBar.Maximum = 0;
            FrameSlider.TrackBar.Value = 0;
            toolStrip.Items.Add(FrameSlider);

            FrameSlider.TrackBar.ValueChanged += OnFrameValueChange;

            m_ProfilerTimer = new Timer();
            m_ProfilerTimer.Interval = ProfilerTimerInterval;
            m_ProfilerTimer.Tick += OnProfilerTimerTick;
        }

        private void CreateColumns()
        {
            treeListView.CanExpandGetter = x => (x as ProfilerNode).Children.Count > 0;
            treeListView.ChildrenGetter = x => (x as ProfilerNode).Children;

            treeListView.FullRowSelect = true;

            var nameColumn = new OLVColumn("Name", "Name");
            nameColumn.AspectGetter = x => (x as ProfilerNode).Name;

            var timeColumn = new OLVColumn("Time (ms)", "Time");
            timeColumn.AspectGetter = x => (x as ProfilerNode).Time;

            treeListView.Columns.Add(nameColumn);
            treeListView.Columns.Add(timeColumn);

            treeListView.Roots = m_Nodes;
        }

        private void TakeSnapshot(object sender, EventArgs e)
        {
            m_NodeDict = Profiler.CopyNodes();
            //ParentNodesWithUnderscores();
            OnFrameValueChange(this, null);
        }

        private void FillProfilerDataForFrame(int index)
        {
            if (m_NodeDict == null)
                return;

            m_Nodes.Clear();

            FillListWithAllNodesForFrame(m_Nodes, index);
            //FillListWithParentOnlyNodesForFrame(m_Nodes, index);

            var selected = treeListView.SelectedItem;
            treeListView.Roots = m_Nodes;
            treeListView.Sort();
            try { treeListView.SelectedItem = selected; }
            catch (ArgumentOutOfRangeException) { treeListView.SelectedItem = null; }

            treeListView.Refresh();
        }

        private void FillListWithAllNodesForFrame(List<ProfilerNode> list, int index)
        {
            foreach (var key in m_NodeDict.Keys)
            {
                if (m_NodeDict[key].Length > index)
                {
                    var node = m_NodeDict[key][index];
                    list.Add(node);
                }
            }
        }

        private void FillListWithParentOnlyNodesForFrame(List<ProfilerNode> list, int index) // Creates recursion which fills the stack
        {
            foreach (var key in m_NodeDict.Keys)
            {
                if (m_NodeDict[key].Length > index)
                {
                    var node = m_NodeDict[key][index];
                    var split = node.Name.Split('_');

                    if (!m_NodeDict.ContainsKey(split[0]) || split.Length == 1)
                        list.Add(node);
                }
            }
        }

        private void ParentNodesWithUnderscores() // It parents classes, which stay in that state after another snapshot, which is bad.
        {
            for (int i = 0; i < Profiler.NodeLimit; ++i)
            {
                foreach (var key in m_NodeDict.Keys)
                {
                    if (m_NodeDict[key].Length > i)
                    {
                        var node = m_NodeDict[key][i];
                        var split = node.Name.Split('_');

                        if (m_NodeDict.ContainsKey(split[0]))
                        {
                            var parentNodes = m_NodeDict[split[0]];
                            if (parentNodes.Length > i)
                                parentNodes[i].Children.Add(node);
                        }
                    }
                }
            }
        }

        private void OnProfilerTimerTick(object sender, EventArgs e)
        {
            if (m_RealTimeProfilingEnabled)
                TakeSnapshot(this, null);
        }

        private void OnRealTimeProfilingButton(object sender, EventArgs e)
        {
            m_RealTimeProfilingEnabled ^= true;
            m_ProfilerTimer.Enabled = m_RealTimeProfilingEnabled;

            if (m_RealTimeProfilingEnabled)
                RealTimeProfilingButton.Text = "Disable Auto Update";
            else
                RealTimeProfilingButton.Text = "Enable Auto Update";
        }

        private void OnFrameValueChange(object sender, EventArgs e)
        {
            var index = -FrameSlider.Value;
            FrameSlider.ToolTipText = -index + " Frame";

            FillProfilerDataForFrame(index);
        }

        private void ProfilerWindow_Resize(object sender, EventArgs e)
        {
            treeListView.Columns[0].Width = (int)(treeListView.Width * 0.78f);
            treeListView.Columns[1].Width = (int)(treeListView.Width * 0.20f);

            FrameSlider.Width = (int)(toolStrip.Width * 0.5f);
            //FrameSlider.Invalidate(); // When resizing, sometimes FrameSlider thinks that it will not fit
        }

        private void ProfilerWindow_ResizeEnd(object sender, EventArgs e)
        {
            FrameSlider.Width = (int)(toolStrip.Width * 0.5f); // Does not help either. toolStrip.Update also did not help
        }

        public ToolStrip ToolStrip { get { return toolStrip; } }
    }
}