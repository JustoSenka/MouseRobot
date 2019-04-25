using Robot;
using RobotEditor.Abstractions;
using RobotRuntime;
using System.Drawing;
using Unity.Lifetime;
using WeifenLuo.WinFormsUI.Docking;

namespace RobotEditor
{
    [RegisterTypeToContainer(typeof(IScreenPreviewWindow), typeof(ContainerControlledLifetimeManager))]
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
