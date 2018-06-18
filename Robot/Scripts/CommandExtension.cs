using Robot.Abstractions;
using RobotRuntime;

namespace Robot
{
    public static class CommandExtension
    {
        public static int GetIndex(this Command command, IScriptManager ScriptManager)
        {
            var script = ScriptManager.GetScriptFromCommand(command);
            var node = script.Commands.GetNodeFromValue(command);
            return node.parent.IndexOf(command);
        }
    }
}
