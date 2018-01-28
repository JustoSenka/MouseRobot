using System;
using System.Drawing;

namespace RobotRuntime.Abstractions
{
    public interface IScreenStateThread : IStableRepeatingThread
    {
        object ScreenBmpLock { get; }

        int Height { get; }
        Bitmap ScreenBmp { get; }
        int Width { get; }

        event Action Initialized;

       // void Init();
    }
}