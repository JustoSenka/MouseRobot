using RobotRuntime;
using RobotRuntime.Graphics;
using RobotRuntime.Perf;
using RobotRuntime.Utils;
using System;
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

            int initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);

            FeatureDetectionThread.Instace.PositionFound += OnPositionFound;

            m_ObservedScreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
            ScreenStateThread.Instace.Initialized += ScreenStateThreadInitialized;
        }

        private void ScreenStateThreadInitialized()
        {
            m_ObservedScreen = new Bitmap(ScreenStateThread.Instace.Width, ScreenStateThread.Instace.Height, PixelFormat.Format32bppArgb);
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


        public static Point[] PointsFrom { get; set; }
        public static Point[] PointsTo { get; set; }

        private Bitmap m_ObservedScreen;
        private object m_ObservedScreenLock = new object();

        private Pen pen = new Pen(Color.Blue, 5);

        protected override void OnPaint(PaintEventArgs e)
        {
            Profiler.Start(typeof(InvisibleForm).ToString() + "_On Paint");
            base.OnPaint(e);

            var g = e.Graphics;

            lock (m_ObservedScreenLock)
            {
                if (m_ObservedScreen != null)
                    g.DrawImage(m_ObservedScreen, new Rectangle(30, 200, 200, 150));
            }

            var count = PointsFrom != null ? "" + PointsFrom.Length : "null";
            g.DrawString("Points found: " + count, Fonts.Big, Brushes.Red, new Point(30, 30));


            if (PointsFrom != null && PointsTo != null)
            {
                var maxLinesToDraw = PointsFrom.Length > 10 ? 10 : PointsFrom.Length;
                for (int i = 0; i < maxLinesToDraw; ++i)
                {
                    g.DrawLine(pen, PointsFrom[i], PointsTo[i]);
                }
            }

            Profiler.Stop(typeof(InvisibleForm).ToString() + "_On Paint");
        }


        private void OnPositionFound(Point[] points)
        {
            Profiler.Start(typeof(InvisibleForm).ToString() + "_Update Positions");
            PointsTo = points;
            PointsFrom = points.Select(p => new Point(5, 5)).ToArray();

            if (m_ObservedScreen != null)
            {
                lock (ScreenStateThread.Instace.ScreenBmpLock)
                    lock (m_ObservedScreenLock)
                    {
                        BitmapUtility.Clone32BPPBitmap(ScreenStateThread.Instace.ScreenBmp, m_ObservedScreen);
                    }
            }

            Invalidate();
            Profiler.Stop(typeof(InvisibleForm).ToString() + "_Update Positions");
        }



        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
