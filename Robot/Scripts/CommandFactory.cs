using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace Robot.Scripts
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
        public const string k_Script = "Script";

        public event Action NewUserCommands;

        public IEnumerable<string> CommandNames { get { return m_CommandNames; } }

        private Type[] m_NativeCommandTypes;
        private Type[] m_UserCommandTypes;

        private string[] m_CommandNames;
        private Dictionary<string, Type> m_CommandTypes;

        private ILogger Logger;
        private IPluginLoader PluginLoader;
        private IUnityContainer Container;
        public CommandFactory(IUnityContainer Container, IPluginLoader PluginLoader, ILogger Logger)
        {
            this.Container = Container;
            this.PluginLoader = PluginLoader;
            this.Logger = Logger;

            PluginLoader.UserDomainReloaded += OnDomainReloaded;

            CollectNativeCommands();
            CollectUserCommands();
        }

        private void OnDomainReloaded()
        {
            CollectUserCommands();
        }

        private void CollectNativeCommands()
        {
            m_NativeCommandTypes = AppDomain.CurrentDomain.GetNativeAssemblies().GetAllTypesWhichImplementInterface(typeof(Command)).ToArray();
        }

        private void CollectUserCommands()
        {
            // DO-DOMAIN: This will not work if assemblies are in different domain
            m_UserCommandTypes = PluginLoader.IterateUserAssemblies(a => a).GetAllTypesWhichImplementInterface(typeof(Command)).ToArray();

            var commandTypeArray = m_NativeCommandTypes.Concat(m_UserCommandTypes).ToArray();

            var dummyCommands = commandTypeArray.Select(type => ((Command)Activator.CreateInstance(type)));
            m_CommandNames = dummyCommands.Select(c => c.Name).ToArray();

            m_CommandTypes = new Dictionary<string, Type>();
            foreach (var command in dummyCommands)
                m_CommandTypes.Add(command.Name, command.GetType());

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
                return (Command)Activator.CreateInstance(typeof(CommandMove));
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
            // DO-DOMAIN: This will not work if command is in other domain
            return m_NativeCommandTypes.Contains(command.GetType());
        }
    }
}
