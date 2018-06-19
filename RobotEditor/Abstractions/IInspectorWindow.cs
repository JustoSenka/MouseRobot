using Robot.Scripts;
using RobotRuntime;

namespace RobotEditor.Abstractions
{
    public interface IInspectorWindow
    {
        void ShowCommand<T>(T command, BaseScriptManager BaseScriptManager) where T : Command;
    }
}