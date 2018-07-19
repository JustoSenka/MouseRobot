using System;
using System.ComponentModel;

namespace Robot.Abstractions
{
    public interface IMouseRobot
    {
        AsyncOperation AsyncOperationOnUI { get; set; }

        bool IsPlaying { get; set; }
        bool IsRecording { get; set; }
        bool IsVisualizationOn { get; set; }

        event Action<bool> PlayingStateChanged;
        event Action<bool> RecordingStateChanged;
        event Action<bool> VisualizationStateChanged;
    }
}