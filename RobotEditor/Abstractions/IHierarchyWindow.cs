using Robot.Abstractions;
using RobotEditor.Hierarchy;
using RobotRuntime.Recordings;
using System;
using System.Windows.Forms;

namespace RobotEditor.Abstractions
{
    public interface IHierarchyWindow
    {
        event Action<HierarchyNode> OnNodeSelected;

        /*event Action<IBaseHierarchyManager, object> OnSelectionChanged;

        void deleteToolStripMenuItem_Click(object sender, EventArgs e);
        void duplicateToolStripMenuItem1_Click(object sender, EventArgs e);
        void newRecordingToolStripMenuItem1_Click(object sender, EventArgs e);
        void SaveAllRecordings();
        void SaveSelectedRecordingWithDialog(Recording recording, bool updateUI = true);

        ToolStrip ToolStrip { get; }*/
    }
}