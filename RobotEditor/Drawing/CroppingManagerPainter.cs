using Robot.Abstractions;
using RobotEditor.Windows.Base;
using RobotRuntime.Graphics;
using RobotRuntime.Utils;
using RobotRuntime.Utils.Win32;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RobotEditor.Drawing
{
    public class CroppingManagerPainter : IPaintOnScreen
    { 
        private ICroppingManager CroppingManager;
        public CroppingManagerPainter(ICroppingManager CroppingManager)
        {
            this.CroppingManager = CroppingManager;

            CroppingManager.ImageCropStarted += OnImageCropStarted;
            CroppingManager.ImageCropEnded += OnImageCropEnded;

            Invalidate?.Invoke();
        }

        public event Action Invalidate;
        public event Action<IPaintOnScreen> StartInvalidateOnTimer;
        public event Action<IPaintOnScreen> StopInvalidateOnTimer;

        public void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            if (CroppingManager.IsCropping)
            {
                DrawCroppingRectangle(g);
            }
        }

        private void OnImageCropStarted(Point p)
        {
            StartInvalidateOnTimer?.Invoke(this);
            Invalidate();
        }

        private void OnImageCropEnded(Point p)
        {
            StopInvalidateOnTimer?.Invoke(this);
            Invalidate();
        }

        private void DrawCroppingRectangle(Graphics g)
        {
            var rect = BitmapUtility.GetRect(CroppingManager.StartPoint, WinAPI.GetCursorPosition());

            rect.Location = rect.Location.Sub(new Point(1, 1));
            rect.Width++;
            rect.Height++;

            g.DrawRectangle(Pens.Black, rect);
        }
    }
}
