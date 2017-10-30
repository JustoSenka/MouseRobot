using System;
namespace RobotRuntime
{
    [Serializable]
    public class LightScript
    {
        public LightScript() { }

        public LightScript(Command[] commands)
        {
            Commands = commands;
        }

        public Command[] Commands { get; protected set; }
    }
}
