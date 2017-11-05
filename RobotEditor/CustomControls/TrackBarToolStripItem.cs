using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace RobotEditor.CustomControls
{
    [System.ComponentModel.DesignerCategory("code")]
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ContextMenuStrip | ToolStripItemDesignerAvailability.MenuStrip)]
    public partial class TrackBarToolStripItem : ToolStripControlHost
    {
        public TrackBarToolStripItem() : base(CreateControlInstance())
        {
            this.AutoSize = false;
        }

        public CustomTrackBar TrackBar
        {
            get
            {
                return Control as CustomTrackBar;
            }
        }

        private static Control CreateControlInstance()
        {
            CustomTrackBar t = new CustomTrackBar();
            t.AutoSize = false;
            // Add other initialization code here.
            return t;
        }

        [DefaultValue(0)]
        public int Value
        {
            get { return TrackBar.Value; }
            set { TrackBar.Value = value; }
        }

        protected override void OnSubscribeControlEvents(Control control)
        {
            base.OnSubscribeControlEvents(control);
            CustomTrackBar CutomTrackbar = control as CustomTrackBar;
            CutomTrackbar.ValueChanged += new EventHandler(CutomTrackbar_ValueChanged);
        }

        protected override void OnUnsubscribeControlEvents(Control control)
        {
            base.OnUnsubscribeControlEvents(control);
            CustomTrackBar CutomTrackbar = control as CustomTrackBar;
            CutomTrackbar.ValueChanged -= new EventHandler(CutomTrackbar_ValueChanged);
        }

        void CutomTrackbar_ValueChanged(object sender, EventArgs e)
        {
            if (this.ValueChanged != null)
            {
                ValueChanged(sender, e);
            }
        }

        public event EventHandler ValueChanged;
        protected override Size DefaultSize
        {
            get
            {
                return new Size(200, 16);
            }
        }
    }
}
