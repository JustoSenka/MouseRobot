using RobotEditor.Windows.Base;
using RobotRuntime;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RobotEditor.Resources.ScriptTemplates
{
    public class CustomScreenPainter : IPaintOnScreen
    {
        // Constructor can ask for dependencies, IAssetManager, IMouseRobot etc.
        public CustomScreenPainter()
        {
            Invalidate.Invoke();
        }

        // Call invalidate if you want OnPaint method to be called. Unless it won't draw anything.
        public event Action Invalidate;

        // These can be used to draw continiuosly. Useful for animations
        public event Action<IPaintOnScreen> StartInvalidateOnTimer;
        public event Action<IPaintOnScreen> StopInvalidateOnTimer;

        public void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.DrawString("Test text", Fonts.Big, Brushes.Red, new PointF(100, 300));
        }
    }
}
