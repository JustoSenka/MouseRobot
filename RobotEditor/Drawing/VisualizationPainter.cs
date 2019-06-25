using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotEditor.Hierarchy;
using RobotEditor.Windows.Base;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Graphics;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobotEditor.Drawing
{
    public class VisualizationPainter : IPaintOnScreen
    {
        private IMouseRobot MouseRobot;
        private IFeatureDetectionThread FeatureDetectionThread;
        private IScreenStateThread ScreenStateThread;
        private ITextDetectionManager TextDetectionManager;
        public VisualizationPainter(IMouseRobot MouseRobot, IFeatureDetectionThread FeatureDetectionThread, IScreenStateThread ScreenStateThread,
            IHierarchyWindow HierarchyWindow, ITextDetectionManager TextDetectionManager)
        {
            this.FeatureDetectionThread = FeatureDetectionThread;
            this.ScreenStateThread = ScreenStateThread;
            this.MouseRobot = MouseRobot;
            this.TextDetectionManager = TextDetectionManager;

            HierarchyWindow.OnNodeSelected += OnNodeSelected;

            FeatureDetectionThread.PositionFound += OnPositionFound;
            ScreenStateThread.Update += OnUpdate;

            m_ObservedScreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);

            Invalidate?.Invoke();
        }

        public event Action Invalidate;
        public event Action<IPaintOnScreen> StartInvalidateOnTimer = delegate { };
        public event Action<IPaintOnScreen> StopInvalidateOnTimer = delegate { };

        public void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            if (MouseRobot.IsVisualizationOn)
            {
                DrawSmallObservedScreenCopy(g);
                DrawPolygonOfMatchedImageBoundaries(g);
                DrawOutlineForEachTextInstance(g);
            }
        }

        private Pen redThinPen = new Pen(Color.Red, 1.5f);
        private Pen bluePen = new Pen(Color.Blue, 3);
        private Pen redPen = new Pen(Color.Red, 3);
        private Pen greenPen = new Pen(Color.Green, 3);

        public Point[][] ImagePoints { get; private set; }
        private object ImagePointsLock = new object();

        private Bitmap m_ObservedScreen;
        private object m_ObservedScreenLock = new object();

        private string m_TextToSearch = "";
        private Point[][] m_TextBlocks = null;
        private object m_TextBlocksLock = new object();

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
                    Pen penToUse = FeatureDetectionThread.WasImageFound ? bluePen : redPen;
                    penToUse = FeatureDetectionThread.WasLastCheckSuccess ? greenPen : penToUse;
                    penToUse = FeatureDetectionThread.TimeSinceLastFind > 3000 ? redPen : penToUse;

                    foreach (var p in ImagePoints)
                    {
                        if (p != null && p.Length > 1) // Should not be needed anymore, used to crash if wrong values are passed
                            g.DrawPolygon(penToUse, p);
                    }
                }
            }
        }

        private void DrawOutlineForEachTextInstance(Graphics g)
        {
            if (m_TextBlocks != null && !string.IsNullOrEmpty(m_TextToSearch))
            {
                lock (m_TextBlocksLock)
                {
                    foreach (var p in m_TextBlocks)
                    {
                        if (p != null && p.Length > 1)
                            g.DrawPolygon(redThinPen, p);
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
            Invalidate?.Invoke();
        }

        private void OnNodeSelected(HierarchyNode node)
        {
            m_TextToSearch = "";
            m_TextBlocks = null;
            Invalidate?.Invoke(); // Clear screen so next screenshot could be taken normally

            if (node == null || node.Command == null)
                return;

            var text = node.Command.GetPropertyIfExist("Text") as string;
            if (string.IsNullOrEmpty(text))
                return;

            m_TextToSearch = text;

            Task.Run(() =>
            {
                var rects = TesseractUtility.GetAllTextBlocks(BitmapUtility.TakeScreenshot());
                var points = rects.Select(p => p.rect.ToPoint()).ToArray();
                lock (m_TextBlocksLock)
                {
                    m_TextBlocks = points;
                }
                Invalidate?.Invoke();
            });

            /*
            TextDetectionManager.FindImageRects(Detectable.FromText(m_TextToSearch), BitmapUtility.TakeScreenshot(), "Tesseract").ContinueWith(async task =>
            {
                var points = await task;
                lock (m_TextBlocksLock)
                {
                    m_TextBlocks = points;
                }
            });*/
        }

        private void OnUpdate()
        {
            if (m_ObservedScreen != null)
            {
                lock (ScreenStateThread.ScreenBmpLock)
                    lock (m_ObservedScreenLock)
                    {
                        BitmapUtility.Clone32BPPBitmap(ScreenStateThread.ScreenBmp, m_ObservedScreen);
                    }
            }
            Invalidate?.Invoke();
        }
    }
}
