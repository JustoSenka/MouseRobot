using Robot.Abstractions;
using RobotRuntime;

namespace Robot
{
    public static class CommandExtension
    {
        public static int GetIndex(this Command command, AbstractCommandCollectionManager AbstractCommandCollectionManager)
        {
            if (AbstractCommandCollectionManager == null)
                return 0;

            var script = AbstractCommandCollectionManager.GetScriptFromCommand(command);
            var node = script.Commands.GetNodeFromValue(command);
            return node.parent.IndexOf(command);
        }

        public static int GetIndex(this Command command, IScriptManager ScriptManager)
        {
            if (ScriptManager == null)
                return 0;

            var script = ScriptManager.GetScriptFromCommand(command);
            var node = script.Commands.GetNodeFromValue(command);
            return node.parent.IndexOf(command);
        }
    }
}
