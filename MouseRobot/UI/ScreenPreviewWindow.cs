using Robot.Graphics;
using Robot.Utils;
using RobotUI.Utils;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotUI
{
    public partial class ScreenPreviewWindow : DockContent
    {
        private bool m_Init;
        private Bitmap m_Bitmap;

        public ScreenPreviewWindow()
        {
            InitializeComponent();

            var screenSize = new Size(ScreenStateThread.Instace.Width, ScreenStateThread.Instace.Height);
            pictureDrawBox.Size = screenSize;

            m_Bitmap = new Bitmap(ScreenStateThread.Instace.Width, ScreenStateThread.Instace.Height, PixelFormat.Format32bppArgb);
            ScreenStateThread.Instace.Update += UpdatePreviewWithNewScreenshot;
            m_Init = true;
        }

        private void UpdatePreviewWithNewScreenshot()
        {
            if (!pictureDrawBox.Created || !m_Init)
                return;

            this.BeginInvoke(new MethodInvoker(delegate
            {
                lock (ScreenStateThread.Instace.ScreenBmpLock)
                {
                    BitmapUtility.Clone32BPPBitmap(ScreenStateThread.Instace.ScreenBmp, m_Bitmap);
                    pictureDrawBox.Image = m_Bitmap;
                    pictureDrawBox.Update();
                }
            }));
        }
    }
}
