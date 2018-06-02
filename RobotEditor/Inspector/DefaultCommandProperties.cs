using Robot.Abstractions;

namespace RobotEditor.Inspector
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
