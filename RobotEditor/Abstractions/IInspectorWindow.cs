using RobotRuntime;

namespace RobotEditor.Abstractions
{
    public interface IInspectorWindow
    {
        void ShowCommand<T>(T command) where T : Command;
    }
}