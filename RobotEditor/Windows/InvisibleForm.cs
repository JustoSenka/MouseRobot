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


        public static Point[] PointsFrom { get; set; }
        public static Point[] PointsTo { get; set; }

        private Bitmap m_ObservedScreen;
        private object m_ObservedScreenLock = new object();

        private Pen pen = new Pen(Color.Blue, 5);

        protected override void OnPaint(PaintEventArgs e)
        {
            RobotRuntime.Perf.Profiler.Start(typeof(InvisibleForm).ToString() + "_On Paint");
            base.OnPaint(e);

            var g = e.Graphics;

            lock (m_ObservedScreenLock)
            {
                if (m_ObservedScreen != null)
                    g.DrawImage(m_ObservedScreen, new Rectangle(20, 150, m_ObservedScreen.Width / 10, m_ObservedScreen.Height / 10));
            }

            var count = PointsFrom != null ? "" + PointsFrom.Length : "null";
            g.DrawString("Points found: " + count, Fonts.Big, Brushes.Red, new Point(30, 30));


            if (PointsFrom != null && PointsTo != null)
            {
                var maxLinesToDraw = PointsFrom.Length > 10 ? 10 : PointsFrom.Length;
                for (int i = 0; i < maxLinesToDraw; ++i)
                {
                    // i % PointsFrom.Length is used because PointsTo can be changed from another thread while running this exact line
                    // Very rare, but no big difference, don't draw the lines just
                    g.DrawLine(pen, PointsFrom[i % PointsFrom.Length], PointsTo[i % PointsFrom.Length]);
                }
            }

            RobotRuntime.Perf.Profiler.Stop(typeof(InvisibleForm).ToString() + "_On Paint");
        }


        private void OnPositionFound(Point[] points)
        {
            RobotRuntime.Perf.Profiler.Start(typeof(InvisibleForm).ToString() + "_Update Positions");
            PointsTo = points;
            PointsFrom = points.Select(p => new Point(5, 5)).ToArray();

            RobotRuntime.Perf.Profiler.Stop(typeof(InvisibleForm).ToString() + "_Update Positions");
            Invalidate();
        }

        private void OnUpdate()
        {
            RobotRuntime.Perf.Profiler.Start(typeof(InvisibleForm).ToString() + "_Update Screen Bitmap");
            if (m_ObservedScreen != null)
            {
                lock (ScreenStateThread.Instace.ScreenBmpLock)
                    lock (m_ObservedScreenLock)
                    {
                        BitmapUtility.Clone32BPPBitmap(ScreenStateThread.Instace.ScreenBmp, m_ObservedScreen);
                    }
            }
            RobotRuntime.Perf.Profiler.Stop(typeof(InvisibleForm).ToString() + "_Update Screen Bitmap");
            Invalidate();
        }



        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
