using Robot.Abstractions;
using RobotRuntime.Abstractions;

namespace RobotEditor.Scripts
{
    /// <summary>
    /// Left empty for future improvements
    /// </summary>
    public class DefaultCommandProperties : CommandProperties
    {
        public DefaultCommandProperties(IScriptManager ScriptManager, ICommandFactory CommandFactory)
            : base(ScriptManager, CommandFactory)
        {
        }
    }
}
