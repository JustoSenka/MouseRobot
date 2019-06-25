using System;
using System.Windows.Forms;
using Robot.Abstractions;

namespace RobotEditor.Abstractions
{
    public interface IBaseHierarchyWindow
    {
        ToolStrip ToolStrip { get; }
        event Action<IBaseHierarchyManager, object> OnSelectionChanged;
    }
}
