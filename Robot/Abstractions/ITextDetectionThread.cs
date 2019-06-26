using System;
using System.Drawing;

namespace Robot.Abstractions
{
    public interface ITextDetectionThread : IStableRepeatingThread
    {
        void StartTextSearch();
        void StopTextSearch();

        event Action TextBlocksFound;

        bool IsLookingForTextBlocks { get; }

        Rectangle[] TextBlocks { get; }
        object TextBlocksLock { get; }

        event Action<string> TextUnderMouseRecognized;
        string LatestTextFound { get; }
        Point LatestCheckedCursorPosition { get; }
        Rectangle RectangleUnderCursor { get; }
    }
}
