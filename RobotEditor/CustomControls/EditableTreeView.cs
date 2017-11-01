using System;
using System.Windows.Forms;

namespace RobotEditor.CustomControls
{
    public class EditableTreeView : TreeView
    {
        public bool IsRenaming { get; protected set; }

        public event Action<NodeLabelEditEventArgs> AfterRename;

        public EditableTreeView() { }

        protected override void OnBeforeLabelEdit(NodeLabelEditEventArgs e)
        {
            base.OnBeforeLabelEdit(e);
            IsRenaming = true;
        }

        protected override void OnAfterLabelEdit(NodeLabelEditEventArgs e)
        {
            base.OnAfterLabelEdit(e);
            IsRenaming = false;

            if (e.Label != "" && e.Label != null)
            {
                e.Node.EndEdit(false);
                try
                {
                    AfterRename.Invoke(e);
                }
                catch (Exception)
                {
                    e.CancelEdit = true;
                }
            }
        }
    }
}
