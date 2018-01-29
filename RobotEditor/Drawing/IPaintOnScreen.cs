using System;
using System.Windows.Forms;

namespace RobotEditor.Windows.Base
{
    /// <summary>
    /// Implement this to draw on screen. DrawOnScreenForm should be up and running
    /// </summary>
    public interface IPaintOnScreen
    {
        /// <summary>
        /// The on paint method will be called using magic by DrawOnScreenForm
        /// </summary>
        void OnPaint(PaintEventArgs e);

        /// <summary>
        /// Remember to call Invalidate if you want to be updated by DrawOnScreenForm. Do not call Invalidate from OnPaint, since it will result into endless recursion
        /// </summary>
        event Action Invalidate;

        /// <summary>
        /// Will start a timer to invalidate view every 30 ms if object is registered
        /// </summary>
        event Action<IPaintOnScreen> StartInvalidateOnTimer;

        /// <summary>
        /// Will stop the timer for this object
        /// </summary>
        event Action<IPaintOnScreen> StopInvalidateOnTimer;
    }
}
