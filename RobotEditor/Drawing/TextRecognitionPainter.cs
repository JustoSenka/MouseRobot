using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotEditor.Windows.Base;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Graphics;
using RobotRuntime.Utils;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobotEditor.Drawing
{
    public class TextRecognitionPainter : IPaintOnScreen
    {
        private readonly IMouseRobot MouseRobot;
        private readonly ITextDetectionManager TextDetectionManager;
        private readonly ITextDetectionThread TextDetectionThread;
        public TextRecognitionPainter(IHierarchyWindow HierarchyWindow, ITextDetectionManager TextDetectionManager,
            ITextDetectionThread TextDetectionThread, IMouseRobot MouseRobot)
        {
            this.MouseRobot = MouseRobot;
            this.TextDetectionManager = TextDetectionManager;
            this.TextDetectionThread = TextDetectionThread;

            HierarchyWindow.OnSelectionChanged += OnNodeSelected;

            TextDetectionThread.TextBlocksFound += () => Invalidate?.Invoke();
            TextDetectionThread.TextUnderMouseRecognized += TextRecognized;
            TextDetectionThread.Update += () => Invalidate?.Invoke();

            Invalidate?.Invoke();
        }

        public event Action Invalidate;
        public event Action<IPaintOnScreen> StartInvalidateOnTimer = delegate { };
        public event Action<IPaintOnScreen> StopInvalidateOnTimer = delegate { };

        public void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            if (MouseRobot.IsTextDetectionOn)
            {
                DrawOutlineForEachTextInstance(g);
                DrawTextUnderCursor(g);
            }
        }

        private Pen redThinPen = new Pen(Color.Red, 1.75f);
        private Pen greenPen = new Pen(Color.Green, 3f);

        private bool m_IsLookingForSpecificText = false;
        private bool m_TextUnderMouseRecognized = false;

        private string m_TextToSearch = "";
        private Rectangle[] m_RecognizedTextBlocks = null;

        private void DrawOutlineForEachTextInstance(Graphics g)
        {
            var font = Fonts.Default;
            var brush = Brushes.Red;
            var textLocation = new PointF(10, 950);
            var recognizedTextLocation = new PointF(10, 920);

            if (TextDetectionThread.IsLookingForTextBlocks)
                g.DrawString("*Searching for text blocks*", font, brush, textLocation);

            if (TextDetectionThread.TextBlocks == null && !TextDetectionThread.IsLookingForTextBlocks)
                g.DrawString("*Did not found any text blocks on screen*", font, brush, textLocation);

            if (m_IsLookingForSpecificText)
                g.DrawString("*Searching for specific text*", font, brush, recognizedTextLocation);

            if (m_RecognizedTextBlocks == null && !m_IsLookingForSpecificText)
                g.DrawString("*Did not found specified text on screen*", font, brush, recognizedTextLocation);

            lock (TextDetectionThread.TextBlocksLock)
            {
                // Draw with red rectangles around all text on screen
                if (TextDetectionThread.TextBlocks != null && !TextDetectionThread.IsLookingForTextBlocks)
                {
                    g.DrawString($"*Found {TextDetectionThread.TextBlocks.Length} text blocks*", font, brush, textLocation);

                    foreach (var r in TextDetectionThread.TextBlocks)
                        g.DrawPolygon(redThinPen, r.ToPoint());
                }

                // Draw rectangle over text which was found from selected command in hierarchy window
                if (m_RecognizedTextBlocks != null && !m_IsLookingForSpecificText)
                {
                    g.DrawString($"*Text was found: {m_TextToSearch}*", font, brush, recognizedTextLocation);

                    foreach (var r in m_RecognizedTextBlocks)
                        g.DrawPolygon(greenPen, r.ToPoint());
                }

                // Draw rectangle over text which is being hovered by mouse
                if (TextDetectionThread.RectangleUnderCursor != default)
                    g.DrawPolygon(greenPen, TextDetectionThread.RectangleUnderCursor.ToPoint());
            }
        }

        private void DrawTextUnderCursor(Graphics g)
        {
            if (m_TextUnderMouseRecognized)
            {
                var p = TextDetectionThread.LatestCheckedCursorPosition.Add(new Point(10, 25));
                g.DrawString(TextDetectionThread.LatestTextFound, Fonts.Normal, Brushes.Black, p);
            }
        }

        private void OnNodeSelected(IBaseHierarchyManager HierarchyManager, object commandOrRecording)
        {
            if (TextDetectionThread.TextBlocks == null || TextDetectionThread.TextBlocks.Length == 0)
                return;

            m_RecognizedTextBlocks = null;
            m_IsLookingForSpecificText = true;
            Invalidate?.Invoke(); // Clear screen so next screenshot could be taken normally

            var text = commandOrRecording?.GetPropertyIfExist("Text") as string;
            if (string.IsNullOrEmpty(text))
                return;

            m_TextToSearch = text;

            Task.Run(() =>
            {
                var blocks = TesseractUtility.GetAllTextFromRects(BitmapUtility.TakeScreenshot(), TextDetectionThread.TextBlocks).ToArray();
                var concat = string.Join(Environment.NewLine, blocks.Select(b => b.text));

                Logger.Log(LogType.Log, $"Recognized text from {blocks.Length} blocks: ", concat);

                var comparer = new TesseractStringEqualityComparer();
                lock (TextDetectionThread.TextBlocksLock)
                {
                    m_RecognizedTextBlocks = blocks.Where(b => comparer.Equals(b.text, m_TextToSearch)).Select(b => b.rect).ToArray();
                }

                m_IsLookingForSpecificText = false;
                Invalidate?.Invoke();
            });

            /*
            TextDetectionManager.FindImageRects(Detectable.FromText(m_TextToSearch), BitmapUtility.TakeScreenshot(), "Tesseract").ContinueWith(async task =>
            {
                var points = await task;
                lock (m_TextBlocksLock)
                {
                    m_TextBlocks = points;
                }
            });*/

        }

        private void TextRecognized(string text)
        {
            m_TextUnderMouseRecognized = !string.IsNullOrEmpty(text);
            Invalidate?.Invoke();
        }
    }
}
