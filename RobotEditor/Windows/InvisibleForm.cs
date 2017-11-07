using RobotRuntime;
using RobotRuntime.Graphics;
using RobotRuntime.Perf;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RobotEditor.Windows
{
    public partial class InvisibleForm : Form
    {
        public static InvisibleForm Instace { get { return m_Instance; } }
        private static InvisibleForm m_Instance = new InvisibleForm();
        private InvisibleForm()
        {
            InitializeComponent();

            this.BackColor = Color.BlanchedAlmond;
            this.TransparencyKey = Color.BlanchedAlmond;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Bounds = Screen.PrimaryScreen.Bounds;
            this.TopMost = true;
            this.ShowInTaskbar = false;

            // Fix flickering when redrawing
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            // Click-through
            int initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);

            FeatureDetectionThread.Instace.PositionFound += OnPositionFound;
            ScreenStateThread.Instace.Update += OnUpdate;

            m_ObservedScreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        private const int WS_EX_TOPMOST = 0x00000008;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= WS_EX_TOPMOST;
                return createParams;
            }
        }


        public Point[][] ImagePoints { get; private set; }
        private object ImagePointsLock = new object();

        private Bitmap m_ObservedScreen;
        private object m_ObservedScreenLock = new object();

        private Pen bluePen = new Pen(Color.Blue, 3);
        private Pen redPen = new Pen(Color.Red, 3);
        private Pen greenPen = new Pen(Color.Green, 3);

        protected override void OnPaint(PaintEventArgs e)
        {
            Profiler.Start("InvisibleForm_OnPaint");
            base.OnPaint(e);

            var g = e.Graphics;

            DrawSmallScreenCopy(g);
            DrawPolygonFromPoints(g);

            Profiler.Stop("InvisibleForm_OnPaint");
        }

        private void DrawSmallScreenCopy(Graphics g)
        {
            lock (m_ObservedScreenLock)
            {
                if (m_ObservedScreen != null)
                    g.DrawImage(m_ObservedScreen, new Rectangle(20, 150, m_ObservedScreen.Width / 10, m_ObservedScreen.Height / 10));
            }
        }

        private void DrawPolygonFromPoints(Graphics g)
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
                        if (p != null && p.Length > 1) // Should not be needed anymore, but crashes if wrong values are passed
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
            Profiler.Start("InvisibleForm_UpdateScreenBitmap");
            if (m_ObservedScreen != null)
            {
                lock (ScreenStateThread.Instace.ScreenBmpLock)
                    lock (m_ObservedScreenLock)
                    {
                        BitmapUtility.Clone32BPPBitmap(ScreenStateThread.Instace.ScreenBmp, m_ObservedScreen);
                    }
            }
            Profiler.Stop("InvisibleForm_UpdateScreenBitmap");
            Invalidate();
        }



        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
