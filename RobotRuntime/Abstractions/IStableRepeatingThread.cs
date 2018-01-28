using System;

namespace RobotRuntime.Abstractions
{
    public interface IStableRepeatingThread
    {
        int FPS { get; set; }
        int FrameTimeMax { get; set; }
        bool IsAlive { get; }

        event Action Update;

        void Init();
        void Join();
        void Start(int FPS = 0);
        void Stop();
    }
}