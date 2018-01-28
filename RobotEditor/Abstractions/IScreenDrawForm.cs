using System.Drawing;

namespace RobotEditor.Abstractions
{
    public interface IScreenDrawForm
    {
        Point[][] ImagePoints { get; }

        Bitmap BlendTwoImagesWithOpacity(Bitmap background, Bitmap front, float opacity);
    }
}