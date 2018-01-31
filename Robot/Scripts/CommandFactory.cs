using RobotRuntime;
using RobotRuntime.Commands;
using System;

namespace Robot.Scripts
{
    public static class CommandFactory
    {
        public const string k_X = "X";
        public const string k_Y = "Y";
        public const string k_DontMove = "DontMove";
        public const string k_Smooth = "Smooth";
        public const string k_Asset = "Asset";
        public const string k_Time = "Time";
        public const string k_Timeout = "Timeout";

        public static Command Create(CommandType commandType)
        {
            switch (commandType)
            {
                case CommandType.Down:
                    return new CommandDown(0, 0, false);
                case CommandType.Move:
                    return new CommandMove(0, 0);
                case CommandType.Press:
                    return new CommandPress(0, 0, false);
                case CommandType.Release:
                    return new CommandRelease(0, 0, false);
                case CommandType.Sleep:
                    return new CommandSleep(0);
                case CommandType.ForImage:
                    return new CommandForImage(default(Guid), 2000);
                case CommandType.ForeachImage:
                    return new CommandForeachImage(default(Guid), 2000);
                default:
                    Logger.Log(LogType.Error, "Not able to create Command with type of: " + commandType);
                    return new CommandSleep(0);
            }
        }

        public static Command Create(CommandType commandType, Command oldCommand)
        {
            var command = Create(commandType);

            CopyPropertiesIfExist(ref command, oldCommand, k_X);
            CopyPropertiesIfExist(ref command, oldCommand, k_Y);
            CopyPropertiesIfExist(ref command, oldCommand, k_DontMove);
            CopyPropertiesIfExist(ref command, oldCommand, k_Smooth);
            CopyPropertiesIfExist(ref command, oldCommand, k_Asset);
            CopyPropertiesIfExist(ref command, oldCommand, k_Time);
            CopyPropertiesIfExist(ref command, oldCommand, k_Timeout);

            return command;
        }

        private static void CopyPropertiesIfExist(ref Command dest, Command source, string name)
        {
            var destProp = dest.GetType().GetProperty(name);
            var sourceProp = source.GetType().GetProperty(name);

            if (destProp != null && sourceProp != null)
            {
                destProp.SetValue(dest, sourceProp.GetValue(source));
            }
        }

        public static void SetPropertyIfExist(ref Command dest, string name, object value)
        {
            var destProp = dest.GetType().GetProperty(name);
            destProp?.SetValue(dest, value);
        }

        public static object GetPropertyIfExist(Command source, string name)
        {
            var prop = source.GetType().GetProperty(name);
            return prop != null ? prop.GetValue(source) : null;
        }
    }
}
