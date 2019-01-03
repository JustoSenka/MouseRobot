using Robot.Recordings;

namespace RobotEditor.Abstractions
{
    public interface IInspectorWindow
    {
        void ShowObject(object obj, BaseHierarchyManager BaseScriptManager = null);
    }
}