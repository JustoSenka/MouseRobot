using RobotRuntime;

namespace Robot
{
    public static class CommandExtension
    {
        public static int GetIndex(this Command command)
        {
            var script = ScriptManager.Instance.GetScriptFromCommand(command);
            var node = script.Commands.GetNodeFromValue(command);
            return node.parent.IndexOf(command);
        }

        public static bool CanBeNested(this Command command)
        {
            return command != null && (command.CommandType == CommandType.ForeachImage || command.CommandType == CommandType.ForImage);
        }
    }
}
