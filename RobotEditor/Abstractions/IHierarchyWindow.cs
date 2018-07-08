using System;
using System.Windows.Forms;
using RobotRuntime.Scripts;
using Robot.Scripts;

namespace RobotEditor.Abstractions
{
    public interface IHierarchyWindow
    {
        event Action<BaseScriptManager, object> OnSelectionChanged;

        void deleteToolStripMenuItem1_Click(object sender, EventArgs e);
        void duplicateToolStripMenuItem1_Click(object sender, EventArgs e);
        void newScriptToolStripMenuItem1_Click(object sender, EventArgs e);
        void SaveAllScripts();
        void SaveSelectedScriptWithDialog(Script script, bool updateUI = true);

        ToolStrip ToolStrip { get; }
    }
}