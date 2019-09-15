using System.Collections.Generic;
using System.Windows.Forms;

namespace RobotEditor.Utils
{
    internal class ToolStripMenuItemComparer : IComparer<ToolStripMenuItem>
    {
        public int Compare(ToolStripMenuItem x, ToolStripMenuItem y)
        {
            return x.Text.CompareTo(y.Text);
        }
    }
}
