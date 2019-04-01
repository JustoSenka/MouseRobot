using Robot.Abstractions;

namespace RobotEditor.Abstractions
{
    public interface IInspectorWindow
    {
        void ShowObject(object obj, IBaseHierarchyManager BaseHierarchyManager = null);
    }
}
