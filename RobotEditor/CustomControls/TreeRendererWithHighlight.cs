using BrightIdeasSoftware;
using RobotEditor.Hierarchy;
using System.Drawing;
using System.Windows.Forms;

namespace RobotEditor.Utils
{
    public class TreeRendererWithHighlight : TreeListView.TreeRenderer
    {
        public TreeRendererWithHighlight() : base()
        {
            IsShowLines = false;
        }

        public override bool RenderSubItem(DrawListViewSubItemEventArgs e, Graphics g, Rectangle r, object x)
        {
            var ret = base.RenderSubItem(e, g, r, x);

            if (x is HierarchyNode node && node.Recording != null)
                g.DrawLine(new Pen(Color.LightGray, 3), new Point(r.Left, r.Top), new Point(r.Right, r.Top));

            return ret;
        }
    }
}
