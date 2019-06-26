using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Utils;
using RobotRuntime.Utils.Win32;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Unity.Lifetime;

namespace Robot.Graphics
{
    [RegisterTypeToContainer(typeof(ITextDetectionThread), typeof(ContainerControlledLifetimeManager))]
    public class TextDetectionThread : StableRepeatingThread, ITextDetectionThread
    {
        protected override string Name { get { return "TextDetectionThread"; } }

        public event Action TextBlocksFound;
        public bool IsLookingForTextBlocks { get; private set; } = false;
        public Rectangle[] TextBlocks { get; private set; } = null;
        public object TextBlocksLock { get; } = new object();

        public event Action<string> TextUnderMouseRecognized;
        public string LatestTextFound { get; private set; }
        public Point LatestCheckedCursorPosition { get; private set; }
        public Rectangle RectangleUnderCursor { get; private set; }

        private Bitmap m_CachedLastScreenshot;

        private readonly ITextDetectionManager TextDetectionManager;
        public TextDetectionThread(ITextDetectionManager TextDetectionManager)
        {
            this.TextDetectionManager = TextDetectionManager;
        }

        public void StartTextSearch()
        {
            IsLookingForTextBlocks = true;
            TextBlocks = null;

            Task.Run(() =>
            {
                m_CachedLastScreenshot = BitmapUtility.TakeScreenshot();
                var rects = TesseractUtility.CollectBlocksWhichContainAnyText(m_CachedLastScreenshot);
                var rectArray = rects.ToArray();
                lock (TextBlocksLock)
                {
                    TextBlocks = rectArray;
                }

                IsLookingForTextBlocks = false;
                TextBlocksFound?.Invoke();
            });
        }

        public void StopTextSearch()
        {
            // This set to true will make the painter not draw anything on the screen. The name is kinda misleading
            IsLookingForTextBlocks = true;

            // Tells to invalidate the screen
            TextBlocksFound?.Invoke();
        }

        protected override void ThreadAction()
        {
            RectangleUnderCursor = default;
            if (TextBlocks == null || TextBlocks.Length == 0)
            {
                TextUnderMouseRecognized?.Invoke("");
                return;
            }

            // Check if cursor is hovering any rectangle/block which contains text
            var cursor = WinAPI.GetCursorPosition();
            RectangleUnderCursor = TextBlocks.FirstOrDefault(b => b.Contains(cursor));

            // If cursor is not hovering any rectangle, early return
            if (RectangleUnderCursor == default)
            {
                TextUnderMouseRecognized?.Invoke("");
                return;
            }

            var tuple = TesseractUtility.GetAllTextFromRects(m_CachedLastScreenshot, new[] { RectangleUnderCursor }).FirstOrDefault();
            if (tuple == default)
            {
                TextUnderMouseRecognized?.Invoke("");
                return;
            }

            LatestCheckedCursorPosition = cursor;
            LatestTextFound = tuple.text;
            TextUnderMouseRecognized?.Invoke(tuple.text);
        }
    }
}
