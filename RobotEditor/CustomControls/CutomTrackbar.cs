using System.Drawing;
using System.Windows.Forms;

namespace RobotEditor.CustomControls
{
    public partial class CustomTrackBar : TrackBar
    {
        public event PaintEventHandler PaintOver;

        public CustomTrackBar() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            // SetStyle(ControlStyles.UserPaint, true); // To make OnPaint to be called
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0F)
            {
                using (Graphics lgGraphics = Graphics.FromHwndInternal(m.HWnd))
                    OnPaintOver(new PaintEventArgs(lgGraphics, this.ClientRectangle));
            }
        }

        protected virtual void OnPaintOver(PaintEventArgs e)
        {
            PaintOver?.Invoke(this, e);

            // I could draw somehting on top of current trackbar here
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);

            // I can disable base paint and draw everything from scratch.. ugh..
        }
    }
}
