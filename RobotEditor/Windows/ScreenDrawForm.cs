using Robot;
using RobotEditor.Windows.Base;
using RobotRuntime.Graphics;
using RobotRuntime.Perf;
using RobotRuntime.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using System;
using RobotEditor.Properties;
using RobotRuntime;

namespace RobotEditor.Windows
{
    public class ScreenDrawForm : InvisibleForm
    {
        public static ScreenDrawForm Instace { get { return m_Instance; } }
        private static ScreenDrawForm m_Instance = new ScreenDrawForm();
        private ScreenDrawForm() : base()
        {
            FeatureDetectionThread.Instace.PositionFound += OnPositionFound;
            ScreenStateThread.Instace.Update += OnUpdate;

            RecordingManager.Instance.ImageFoundInAssets += OnImageFoundInAssets;
            RecordingManager.Instance.ImageNotFoundInAssets += OnImageNotFoundInAssets;

            m_ObservedScreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);

            m_FindImageTimer.Tick += OnFindImageTimerTick;
            m_FindImageTimer.Interval = 30;
        }

        private Pen bluePen = new Pen(Color.Blue, 3);
        private Pen redPen = new Pen(Color.Red, 3);
        private Pen greenPen = new Pen(Color.Green, 3);

        protected override void OnPaint(PaintEventArgs e)
        {
            Profiler.Start("InvisibleForm_OnPaint");
            base.OnPaint(e);

            var g = e.Graphics;

            if (MouseRobot.Instance.IsVisualizationOn)
            {
                DrawSmallObservedScreenCopy(g);
                DrawPolygonOfMatchedImageBoundaries(g);
            }

            if (m_FindImageWatch.ElapsedMilliseconds < k_ImageShowLength && m_LastCursorPos != default(Point))
            {
                var rect = DrawFoundImageUnderCursor(g);
                DrawImageAssetTextUnderCursor(g, rect);
            }

            Profiler.Stop("InvisibleForm_OnPaint");
        }

        #region Image Detection Visualization

        public Point[][] ImagePoints { get; private set; }
        private object ImagePointsLock = new object();

        private Bitmap m_ObservedScreen;
        private object m_ObservedScreenLock = new object();

        private void DrawSmallObservedScreenCopy(Graphics g)
        {
            lock (m_ObservedScreenLock)
            {
                if (m_ObservedScreen != null)
                    g.DrawImage(m_ObservedScreen, new Rectangle(20, 150, m_ObservedScreen.Width / 10, m_ObservedScreen.Height / 10));
            }
        }

        private void DrawPolygonOfMatchedImageBoundaries(Graphics g)
        {
            if (ImagePoints != null)
            {
                lock (ImagePointsLock)
                {
                    Pen penToUse = FeatureDetectionThread.Instace.WasImageFound ? bluePen : redPen;
                    penToUse = FeatureDetectionThread.Instace.WasLastCheckSuccess ? greenPen : penToUse;
                    penToUse = FeatureDetectionThread.Instace.TimeSinceLastFind > 3000 ? redPen : penToUse;

                    foreach (var p in ImagePoints)
                    {
                        if (p != null && p.Length > 1) // Should not be needed anymore, used to crash if wrong values are passed
                            g.DrawPolygon(penToUse, p);
                    }
                }
            }
        }

        private void OnPositionFound(IEnumerable<Point[]> points)
        {
            lock (ImagePointsLock)
            {
                ImagePoints = points.ToArray();
            }
            Invalidate();
        }

        private void OnUpdate()
        {
            if (m_ObservedScreen != null)
            {
                lock (ScreenStateThread.Instace.ScreenBmpLock)
                    lock (m_ObservedScreenLock)
                    {
                        BitmapUtility.Clone32BPPBitmap(ScreenStateThread.Instace.ScreenBmp, m_ObservedScreen);
                    }
            }
            Invalidate();
        }

        #endregion

        #region Recording Manager Visual

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
            Invalidate();
        }

        private void OnImageNotFoundInAssets(Point point)
        {
            m_FindImageWatch.Restart();
            m_AssetUnderCursor = null;
            m_LastCursorPos = point;
            m_FindImageTimer.Enabled = true;
            m_LastScreeBmpAtPos = null;
            Invalidate();
        }

        private Rectangle DrawFoundImageUnderCursor(Graphics g)
        {
            var bmp = (m_AssetUnderCursor != null) ? m_AssetUnderCursor.Importer.Load<Bitmap>() : Resources.X_ICO_256;
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
                m_FindImageTimer.Enabled = false;

            Invalidate();
        }

        #endregion

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
