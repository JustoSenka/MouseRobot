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
        void newScriptToolStripMenuItem1_Click(object sender, EventArgs e);
        void SaveAllScripts();
        void SaveSelectedScriptWithDialog(Recording script, bool updateUI = true);

        ToolStrip ToolStrip { get; }
    }
}