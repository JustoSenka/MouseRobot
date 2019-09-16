using System.Collections.Generic;
using System.Windows.Forms;

namespace RobotEditor.Abstractions
{
    public interface IFontManager
    {
        IList<Form> Forms { get; }
        IList<Control> Controls { get; }

        void ForceUpdateFonts();
    }
}