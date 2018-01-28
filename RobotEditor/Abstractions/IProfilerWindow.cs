using System.Windows.Forms;
using RobotEditor.CustomControls;

namespace RobotEditor.Abstractions
{
    public interface IProfilerWindow
    {
        TrackBarToolStripItem FrameSlider { get; }
        ToolStripButton RealTimeProfilingButton { get; }
    }
}