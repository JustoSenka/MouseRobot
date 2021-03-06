﻿using RobotRuntime.Commands;
using System;
using System.Linq;
using System.Reflection;

namespace RobotRuntime.IO
{
    public static class YamlCommandIO
    {
        private const BindingFlags k_BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        public static TreeNode<YamlObject> Serialize(Command command, int level)
        {
            var commandObject = new YamlObject(level, command.GetType().Name, "");
            var tree = new TreeNode<YamlObject>(commandObject);

            tree.AddChild(new YamlObject(level + 1, "Guid", command.Guid));
            tree.AddChild(new YamlObject(level + 1, "OverrideTitle", command.OverrideTitle));

            var objs = YamlSerializer.SerializeSimpleProperties(command, level + 1);

            foreach (var o in objs)
                tree.AddChild(o);

            return tree;
        }

        public static Command Deserialize(TreeNode<YamlObject> tree)
        {
            //var amIDesiarializingCommandType = tree.value.value == "";

            var amIDesiarializingCommandType = tree.value.value == "" && // Command does not have right side value
                tree.Count() > 0; // Has children (at least one, guid)

            if (amIDesiarializingCommandType)
            {
                // TODO: These two lines take a lot of time, and they will be called a lot of times!!!
#pragma warning disable CS0618
                var allCommandTypes = AppDomain.CurrentDomain.GetAllAssemblies().GetAllTypesWhichImplementInterface(typeof(Command));
#pragma warning restore CS0618

                Command command;

                var commandType = allCommandTypes.FirstOrDefault(type => type.Name.Equals(tree.value.property));
                if (commandType == null)
                {
                    Logger.Log(LogType.Error, $"Cannot deserialize command: {tree.value.property}, unknown type, skipping this command.");

                    var commandText = string.Join(Environment.NewLine, tree.Select(t => t.value.ToString()).Prepend(tree.value.ToString()));
                    command = new CommandUnknown(commandText);
                }
                else
                {
                    command = (Command)Activator.CreateInstance(commandType);
                    YamlSerializer.DeserializeSimpleProperties(command, tree);
                }

                return command;
            }

            // Skipping command fields here since they are deserialized with simple properties deserializer together with command above
            return null;
        }
    }
}
