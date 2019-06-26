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
        private IMouseRobot MouseRobot;
        private ITextDetectionManager TextDetectionManager;
        public TextRecognitionPainter(IMouseRobot MouseRobot, IHierarchyWindow HierarchyWindow, ITextDetectionManager TextDetectionManager)
        {
            this.MouseRobot = MouseRobot;
            this.TextDetectionManager = TextDetectionManager;

            HierarchyWindow.OnSelectionChanged += OnNodeSelected;
            MouseRobot.TextDetectionStateChanged += TextDetectionStateChanged;

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
            }
        }

        private Pen redThinPen = new Pen(Color.Red, 1.75f);
        private Pen greenPen = new Pen(Color.Green, 3f);

        private bool m_IsLookingForTextBlocks = false;
        private bool m_IsLookingForSpecificText = false;

        private string m_TextToSearch = "";
        private Rectangle[] m_TextBlocks = null;
        private Rectangle[] m_RecognizedTextBlocks = null;
        private readonly object m_TextBlocksLock = new object();

        private void DrawOutlineForEachTextInstance(Graphics g)
        {
            var font = Fonts.Default;
            var brush = Brushes.Red;
            var textLocation = new PointF(10, 950);
            var recognizedTextLocation = new PointF(10, 920);

            if (m_IsLookingForTextBlocks)
                g.DrawString("*Searching for text blocks*", font, brush, textLocation);

            if (m_TextBlocks == null && !m_IsLookingForTextBlocks)
                g.DrawString("*Did not found any text blocks on screen*", font, brush, textLocation);

            if (m_IsLookingForSpecificText)
                g.DrawString("*Searching for specific text*", font, brush, recognizedTextLocation);

            if (m_RecognizedTextBlocks == null && !m_IsLookingForSpecificText)
                g.DrawString("*Did not found specified text on screen*", font, brush, recognizedTextLocation);

            lock (m_TextBlocksLock)
            {
                if (m_TextBlocks != null && !m_IsLookingForTextBlocks)
                {
                    g.DrawString($"*Found {m_TextBlocks.Length} text blocks*", font, brush, textLocation);

                    foreach (var r in m_TextBlocks)
                        g.DrawPolygon(redThinPen, r.ToPoint());
                }

                if (m_RecognizedTextBlocks != null && !m_IsLookingForSpecificText) 
                {
                    g.DrawString($"*Text was found: {m_TextToSearch}*", font, brush, recognizedTextLocation);

                    foreach (var r in m_RecognizedTextBlocks)
                        g.DrawPolygon(greenPen, r.ToPoint());
                }
            }
        }

        private void TextDetectionStateChanged(bool enabled)
        {
            if (enabled)
            {
                m_IsLookingForTextBlocks = true;
                m_TextBlocks = null;
                Invalidate?.Invoke(); // Clear screen so next screenshot could be taken normally

                Task.Run(() =>
                {
                    var rects = TesseractUtility.CollectBlocksWhichContainAnyText(BitmapUtility.TakeScreenshot());
                    var rectArray = rects.ToArray();
                    lock (m_TextBlocksLock)
                    {
                        m_TextBlocks = rectArray;
                    }

                    m_IsLookingForTextBlocks = false;
                    Invalidate?.Invoke();
                });
            }
            else
            {
                m_IsLookingForTextBlocks = true;
                Invalidate?.Invoke();
            }
        }

        private void OnNodeSelected(IBaseHierarchyManager HierarchyManager, object commandOrRecording)
        {
            if (m_TextBlocks == null || m_TextBlocks.Length == 0)
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
                var blocks = TesseractUtility.GetAllTextFromRects(BitmapUtility.TakeScreenshot(), m_TextBlocks).ToArray();
                var concat = string.Join(Environment.NewLine, blocks.Select(b => b.text));

                Logger.Log(LogType.Log, $"Recognized text from {blocks.Length} blocks: ", concat);

                var comparer = new TesseractStringEqualityComparer();
                lock (m_TextBlocksLock)
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
    }
}
