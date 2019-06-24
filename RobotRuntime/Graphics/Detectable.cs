using System.Diagnostics;
using System.Drawing;

namespace RobotRuntime
{
    public struct Detectable
    {
        public object Value { get; private set; }
        public DetectableType DetectableType { get; private set; }

        public Detectable(object Value, DetectableType DetectableType)
        {
            Trace.Assert(Value != null, "Detectable value should never be null. Did asset failed to load?");

            this.Value = Value;
            this.DetectableType = DetectableType;
        }

        public static Detectable FromBitmap(Bitmap bitmap) => new Detectable(bitmap, DetectableType.Image);
        public static Detectable FromText(string text) => new Detectable(text, DetectableType.SpecificText);
        public static Detectable AllTextOnScreen => new Detectable("Detect All Text", DetectableType.TextBlocks);
    }

    public enum DetectableType
    {
        Image, SpecificText, TextBlocks
    }
}
