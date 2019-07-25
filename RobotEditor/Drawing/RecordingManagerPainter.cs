using Robot;
using Robot.Abstractions;
using RobotEditor.Properties;
using RobotEditor.Windows.Base;
using RobotRuntime;
using RobotRuntime.Graphics;
using RobotRuntime.Utils;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace RobotEditor.Drawing
{
    public class RecordingManagerPainter : IPaintOnScreen
    { 
        private IRecordingManager RecordingManager;
        public RecordingManagerPainter(IRecordingManager RecordingManager)
        {
            this.RecordingManager = RecordingManager;

            RecordingManager.ImageFoundInAssets += OnImageFoundInAssets;
            RecordingManager.ImageNotFoundInAssets += OnImageNotFoundInAssets;

            m_FindImageTimer.Tick += OnFindImageTimerTick;
            m_FindImageTimer.Interval = 30;

            Invalidate?.Invoke();
        }

        public event Action Invalidate;
        public event Action<IPaintOnScreen> StartInvalidateOnTimer;
        public event Action<IPaintOnScreen> StopInvalidateOnTimer;

        public void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            if (m_FindImageWatch.ElapsedMilliseconds < k_ImageShowLength && m_LastCursorPos != default(Point))
            {
                var rect = DrawFoundImageUnderCursor(g);
                DrawImageAssetTextUnderCursor(g, rect);
            }
        }

        private Stopwatch m_FindImageWatch = new Stopwatch();
        private Timer m_FindImageTimer = new Timer();
        private Asset m_AssetUnderCursor;
        private Point m_LastCursorPos;
        private Bitmap m_LastScreeBmpAtPos;
        private const int k_ImageShowLength = 1200;
        private const int k_MaxImagePreviewSize = 100;

        private void OnImageFoundInAssets(Asset asset, Point point)
        {
            m_FindImageWatch.Restart();
            m_AssetUnderCursor = asset;
            m_LastCursorPos = point;
            m_FindImageTimer.Enabled = true;
            m_LastScreeBmpAtPos = null;
            StartInvalidateOnTimer?.Invoke(this);
            Invalidate?.Invoke();
        }

        private void OnImageNotFoundInAssets(Point point)
        {
            m_FindImageWatch.Restart();
            m_AssetUnderCursor = null;
            m_LastCursorPos = point;
            m_FindImageTimer.Enabled = true;
            m_LastScreeBmpAtPos = null;
            StartInvalidateOnTimer?.Invoke(this);
            Invalidate?.Invoke();
        }

        private Rectangle DrawFoundImageUnderCursor(Graphics g)
        {
            var bmp = (m_AssetUnderCursor != null) ? m_AssetUnderCursor.Load<Bitmap>() : Properties.Resources.X_ICO_256;
            var ratio = k_MaxImagePreviewSize * 1.0f / (bmp.Width > bmp.Height ? bmp.Width : bmp.Height);
            ratio *= (m_AssetUnderCursor != null) ? 1 : 0.4f;

            var rect = new Rectangle(m_LastCursorPos, new Size((int)(bmp.Width * ratio), (int)(bmp.Height * ratio)));
            var opacity = GetOpacityValue((int)m_FindImageWatch.ElapsedMilliseconds, k_ImageShowLength);

            if (m_LastScreeBmpAtPos == null)
                m_LastScreeBmpAtPos = BitmapUtility.TakeScreenshotOfSpecificRect(rect.Location, rect.Size);

            var bmp2 = BitmapUtility.ResizeBitmap(bmp, rect);
            var bmp3 = BlendTwoImagesWithOpacity(m_LastScreeBmpAtPos, bmp2, opacity);
            g.DrawImage(bmp3, rect);
            return rect;
        }

        private void DrawImageAssetTextUnderCursor(Graphics g, Rectangle rectOfImageRef)
        {
            if (m_AssetUnderCursor != null)
            {
                var p = rectOfImageRef.Location.Add(new Point(5, rectOfImageRef.Height + 10));
                g.DrawString(m_AssetUnderCursor.Name, Fonts.Normal, Brushes.Black, p);
            }
        }

        private void OnFindImageTimerTick(object sender, EventArgs e)
        {
            if (m_FindImageWatch.ElapsedMilliseconds > k_ImageShowLength)
            {
                m_FindImageTimer.Enabled = false;
                StopInvalidateOnTimer?.Invoke(this);
            }
        }

        public Bitmap BlendTwoImagesWithOpacity(Bitmap background, Bitmap front, float opacity)
        {
            var bmp = new Bitmap(front.Width, front.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                var matrix = new ColorMatrix();
                var attr = new ImageAttributes();
                matrix.Matrix33 = opacity;
                attr.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                g.DrawImage(background, Point.Empty);
                g.DrawImage(front, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, front.Width, front.Height, GraphicsUnit.Pixel, attr);
            }
            return bmp;
        }

        private float GetOpacityValue(int elapsedMilliseconds, int m_ImageShowLength)
        {
            var low = m_ImageShowLength * 1.0f * 4 / 10;
            var high = m_ImageShowLength * 1.0f * 6 / 10;

            if (elapsedMilliseconds < low)
                return elapsedMilliseconds / low;
            else if (elapsedMilliseconds > high)
                return (m_ImageShowLength - elapsedMilliseconds * 1.0f) / (m_ImageShowLength - high);
            else
                return 1;
        }
    }
}
