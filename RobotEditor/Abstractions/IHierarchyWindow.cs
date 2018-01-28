using System;
using Robot.Scripts;
using RobotRuntime;

namespace RobotEditor.Abstractions
{
    public interface IHierarchyWindow
    {
        event Action<Command> OnCommandSelected;

        void deleteToolStripMenuItem1_Click(object sender, EventArgs e);
        void duplicateToolStripMenuItem1_Click(object sender, EventArgs e);
        void newScriptToolStripMenuItem1_Click(object sender, EventArgs e);
        void SaveAllScripts();
        void SaveSelectedScriptWithDialog(Script script, bool updateUI = true);
    }
}