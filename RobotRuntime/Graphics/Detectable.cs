using System.Diagnostics;
using System.Drawing;

namespace RobotRuntime
{
    public struct Detectable
    {
        public object Value { get; private set; }
        public float Threshold { get; private set; }
        public DetectableType DetectableType { get; private set; }

        public Detectable(object Value, DetectableType DetectableType, float Threshold = 0.8f)
        {
            Trace.Assert(Value != null, "Detectable value should never be null. Did asset failed to load?");

            this.Value = Value;
            this.DetectableType = DetectableType;
            this.Threshold = Threshold;
        }

        public static Detectable FromBitmap(Bitmap bitmap, float Threshold = 0.8f) => new Detectable(bitmap, DetectableType.Image, Threshold);
        public static Detectable FromText(string text, float Threshold = 0.8f) => new Detectable(text, DetectableType.SpecificText, Threshold);
        public static Detectable AllTextOnScreen => new Detectable("Detect All Text", DetectableType.TextBlocks);
    }

    public enum DetectableType
    {
        Image, SpecificText, TextBlocks
    }
}
