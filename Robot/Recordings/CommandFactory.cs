using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Robot.Recordings
{
    public class CommandFactory : ICommandFactory
    {
        public const string k_X = "X";
        public const string k_Y = "Y";
        public const string k_DontMove = "DontMove";
        public const string k_Smooth = "Smooth";
        public const string k_Asset = "Asset";
        public const string k_Time = "Time";
        public const string k_Timeout = "Timeout";
        public const string k_Recording = "Recording";

        public event Action NewUserCommands;

        public IEnumerable<string> CommandNames { get { return m_CommandNames; } }

        private string[] m_CommandNames;
        private Dictionary<string, Type> m_CommandTypes;

        private ILogger Logger;
        private readonly ITypeCollector<Command> TypeCollector;
        public CommandFactory(ILogger Logger, ITypeCollector<Command> TypeCollector)
        {
            this.Logger = Logger;
            this.TypeCollector = TypeCollector;

            TypeCollector.NewTypesAppeared += CollectUserCommands;
        }

        private void CollectUserCommands()
        {
            var dummyCommands = TypeCollector.AllTypes.Select(type => ((Command)Activator.CreateInstance(type))).ToArray();
            m_CommandNames = dummyCommands.Select(c => c.Name).ToArray();

            m_CommandTypes = new Dictionary<string, Type>();
            foreach (var command in dummyCommands)
            {
                if (Logger.AssertIf(m_CommandTypes.ContainsKey(command.Name),
                    "Command of type '" + command.GetType() +
                    "' cannot be added because other command with same name already exists: " + command.Name))
                    continue;

                m_CommandTypes.Add(command.Name, command.GetType());
            }

            NewUserCommands?.Invoke();
        }

        public Command Create(string commandName)
        {
            if (m_CommandTypes.ContainsKey(commandName))
            {
                var type = m_CommandTypes[commandName];
                return (Command)Activator.CreateInstance(type);
            }
            else
            {
                Logger.Logi(LogType.Error, "Command with name '" + commandName + "' not found.", "Returning first command on the list.");
                return new CommandSleep(0);
            }
        }

        public Command Create(string commandName, Command oldCommand)
        {
            var command = Create(commandName);

            command.CopyAllProperties(oldCommand);
            command.CopyAllFields(oldCommand);

            /*
            command.CopyPropertyFromIfExist(oldCommand, k_X);
            command.CopyPropertyFromIfExist(oldCommand, k_Y);
            command.CopyPropertyFromIfExist(oldCommand, k_DontMove);
            command.CopyPropertyFromIfExist(oldCommand, k_Smooth);
            command.CopyPropertyFromIfExist(oldCommand, k_Asset);
            command.CopyPropertyFromIfExist(oldCommand, k_Time);
            command.CopyPropertyFromIfExist(oldCommand, k_Timeout);*/

            return command;
        }

        public bool IsNative(Command command)
        {
            return TypeCollector.IsNative(command.GetType());
        }
    }
}
