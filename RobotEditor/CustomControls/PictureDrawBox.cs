using RobotRuntime.Graphics;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RobotEditor.CustomControls
{
    public class PictureDrawBox : PictureBox
    {
        private const float k_ZoomIntensity = 0.12f;

        private float m_Zoom = 1;

        private bool m_IsBeingDragged = false;

        private PointF m_MouseDownPos;
        private PointF m_ImagePos;
        private PointF m_OldImagePos;

        public PictureDrawBox() : base()
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Image == null)
                return;

            Graphics g = e.Graphics;

            g.ScaleTransform(m_Zoom, m_Zoom);

            //g.TranslateTransform(, m_ImagePos.Y);// Used to give the same result, not sure which is more correct way of doing things

            g.DrawImage(this.Image, m_ImagePos);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                m_IsBeingDragged = true;

                m_OldImagePos = m_ImagePos;
                m_MouseDownPos = e.Location;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (m_IsBeingDragged)
            {
                m_ImagePos = GetNewImagePosition(e.Location);
                m_OldImagePos = m_ImagePos;
                Invalidate();
                m_IsBeingDragged = false;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (m_IsBeingDragged)
            {
                m_ImagePos = GetNewImagePosition(e.Location);
                Invalidate();
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            var oldZoomValue = m_Zoom;
            if (e.Delta > 0)
            {
                // Zoom in
                m_Zoom *= 1 + k_ZoomIntensity;
                m_ImagePos = m_ImagePos.Sub(GetImagePositionDeltaFromScroll(e.Location, k_ZoomIntensity));
            }
            else
            {
                // Zoom out
                m_Zoom *= 1 / (1 + k_ZoomIntensity); // m_Zoom /= 1 + k_ZoomIntensity, or basically opposite of zooming in
                var zoomDelta = 1 - 1 / (1 + k_ZoomIntensity); // percentage of how smaller the image should become
                m_ImagePos = m_ImagePos.Add(GetImagePositionDeltaFromScroll(e.Location, zoomDelta));
            }

            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Cursor = Cursors.Arrow;
        }

        private PointF GetNewImagePosition(Point currentMousePos)
        {
            return new PointF
            {
                X = m_OldImagePos.X + ((currentMousePos.X - m_MouseDownPos.X) * (1 / m_Zoom)),
                Y = m_OldImagePos.Y + ((currentMousePos.Y - m_MouseDownPos.Y) * (1 / m_Zoom))
            };
        }

        private PointF GetImagePositionDeltaFromScroll(Point currentMousePos, float zoomDelta)
        {
            //var screenMiddleX = Width / m_Zoom / 2; Can use those if wanted to keep image always in center
            //var screenMiddleY = Height / m_Zoom / 2;

            var mousePosInFieldX = currentMousePos.X / m_Zoom;
            var mousePosInFieldY = currentMousePos.Y / m_Zoom;

            var defaultMoveDueToScalingWithTopLeftPivotX = mousePosInFieldX * zoomDelta;
            var defaultMoveDueToScalingWithTopLeftPivotY = mousePosInFieldY * zoomDelta;

            return new PointF
            {
                X = defaultMoveDueToScalingWithTopLeftPivotX,
                Y = defaultMoveDueToScalingWithTopLeftPivotY
            };
        }

        public void CenterPosition()
        {
            m_ImagePos.X = Width / 2 - Image.Width / 2;
            m_ImagePos.Y = Height / 2 - Image.Height / 2;

            m_Zoom = 1;
            // TODO: Adjust zoom level so image fits the screen
        }

        public new Image Image
        {
            get
            {
                return base.Image;
            }
            set
            {
                if (value == null)
                    return;

                base.Image = value;
            }
        }

        public PointF ImagePosition
        {
            get
            {
                return m_ImagePos;
            }
            set
            {
                m_ImagePos = value;
            }
        }
    }
}
