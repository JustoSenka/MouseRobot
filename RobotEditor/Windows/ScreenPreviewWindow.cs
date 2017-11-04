using Robot;
using RobotRuntime.Graphics;
using RobotRuntime.Utils;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor
{
    public partial class ScreenPreviewWindow : DockContent
    {
        private bool m_Init;
        private Bitmap m_ScreenBitmap;
        private Asset m_Asset;

        public ScreenPreviewWindow()
        {
            InitializeComponent();

            //ScreenStateThread.Instace.Initialized += ScreenStateThreadInitialized;

            m_Init = true;
        }

        public void Preview(Asset asset)
        {
            if (!asset.HoldsTypeOf(typeof(Bitmap)))
                return;


            pictureDrawBox.Image = asset.Importer.Load<Bitmap>();
            if (m_Asset != asset)
                pictureDrawBox.CenterPosition();

            pictureDrawBox.Update();
            m_Asset = asset;
        }

        private void ScreenStateThreadInitialized()
        {
            m_ScreenBitmap = new Bitmap(ScreenStateThread.Instace.Width, ScreenStateThread.Instace.Height, PixelFormat.Format32bppArgb);
            ScreenStateThread.Instace.Update += UpdatePreviewWithNewScreenshot;
        }

        private void UpdatePreviewWithNewScreenshot()
        {
            if (!pictureDrawBox.Created || !m_Init)
                return;

            this.BeginInvoke(new MethodInvoker(delegate
            {
                lock (ScreenStateThread.Instace.ScreenBmpLock)
                {
                    BitmapUtility.Clone32BPPBitmap(ScreenStateThread.Instace.ScreenBmp, m_ScreenBitmap);
                }
                pictureDrawBox.Image = m_ScreenBitmap;
                pictureDrawBox.Update();
            }));
        }
    }
}
