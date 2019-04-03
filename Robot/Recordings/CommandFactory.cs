using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
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
            CollectUserCommands();
        }

        private void CollectUserCommands()
        {
            var dummyCommands = TypeCollector.AllTypes
                .Where(t => t != typeof(CommandUnknown))
                .TryResolveTypes<Command>(null, Logger).ToArray();

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

        /// <summary>
        /// Creates command from Command Name
        /// If type could not be loaded returns null
        /// </summary>
        public Command Create(string commandName)
        {
            if (m_CommandTypes.ContainsKey(commandName))
            {
                var type = m_CommandTypes[commandName];
                try
                {
                    return (Command)Activator.CreateInstance(type);
                }
                catch (Exception e)
                {
                    Logger.Logi(LogType.Error, "Command with name '" + commandName + "' could not be instantiated: " + e.Message);
                    return null;
                }
            }
            else
            {
                Logger.Logi(LogType.Error, "Command with name '" + commandName + "' not found.");
                return null;
            }
        }

        /// <summary>
        /// Creates command with values of an old command.
        /// If type could not be loaded returns null
        /// </summary>
        public Command Create(string commandName, Command oldCommand)
        {
            var command = Create(commandName);
            if (command == null)
                return null;

            command.CopyAllProperties(oldCommand);
            command.CopyAllFields(oldCommand);

            return command;
        }

        public bool IsNative(Command command)
        {
            return TypeCollector.IsNative(command.GetType());
        }
    }
}
