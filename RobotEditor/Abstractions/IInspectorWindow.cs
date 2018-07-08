using Robot.Scripts;

namespace RobotEditor.Abstractions
{
    public interface IInspectorWindow
    {
        void ShowObject(object obj, BaseScriptManager BaseScriptManager = null);
    }
}