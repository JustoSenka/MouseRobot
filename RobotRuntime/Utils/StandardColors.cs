using System.Drawing;

namespace RobotRuntime.Utils
{
    /// <summary>
    /// Standard colors followed on microsoft products.
    /// https://www.microsoft.com/en-us/design/color
    /// 
    /// Blue is visual studio Status Strip color
    /// Orange is visual studio Status Strip color when debugging
    /// </summary>
    public static class StandardColors
    {
        public static Color Default { get { return default(Color); } }

        public static Color Blue { get { return Color.FromArgb(unchecked((int)0xFF007ACC)); } }
        public static Color Orange { get { return Color.FromArgb(unchecked((int)0xFFCA5100)); } }
        public static Color Red { get { return Color.FromArgb(unchecked((int)0xFFC30052)); } }
        public static Color Green { get { return Color.FromArgb(unchecked((int)0xFF107C10)); } }
        public static Color Purple { get { return Color.FromArgb(unchecked((int)0xFF800074)); } }
        public static Color DarkGrey { get { return Color.FromArgb(unchecked((int)0xFF393939)); } }
    }
}
