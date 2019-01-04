using System;
using System.Windows.Forms;
using RobotRuntime.Recordings;
using Robot.Recordings;

namespace RobotEditor.Abstractions
{
    public interface IHierarchyWindow
    {
        event Action<BaseHierarchyManager, object> OnSelectionChanged;

        void deleteToolStripMenuItem1_Click(object sender, EventArgs e);
        void duplicateToolStripMenuItem1_Click(object sender, EventArgs e);
        void newRecordingToolStripMenuItem1_Click(object sender, EventArgs e);
        void SaveAllRecordings();
        void SaveSelectedRecordingWithDialog(Recording recording, bool updateUI = true);

        ToolStrip ToolStrip { get; }
    }
}