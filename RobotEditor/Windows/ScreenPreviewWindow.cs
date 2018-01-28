using Robot;
using RobotEditor.Abstractions;
using System.Drawing;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor
{
    public partial class ScreenPreviewWindow : DockContent, IScreenPreviewWindow
    {
        private Asset m_Asset;

        public ScreenPreviewWindow()
        {
            InitializeComponent();
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
    }
}
