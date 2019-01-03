using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Recordings;

namespace Robot
{
    public static class CommandExtension
    {
        public static int GetIndex(this Command command, IHierarchyManager ScriptManager)
        {
            var script = ScriptManager.GetScriptFromCommand(command);
            var node = script.Commands.GetNodeFromValue(command);
            return node.parent.IndexOf(command);
        }

        public static int GetIndex(this Command command, RobotRuntime.Recordings.Recording script)
        {
            var node = script.Commands.GetNodeFromValue(command);
            return node.parent.IndexOf(command);
        }

        public static int GetIndex(this RobotRuntime.Recordings.Recording script, IHierarchyManager ScriptManager)
        {
            return ScriptManager.LoadedScripts.IndexOf(script);
        }
    }
}
