using RobotRuntime.Utils;
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

            var objs = YamlSerializer.SerializeSimpleProperties(command, level + 1);

            foreach (var o in objs)
                tree.AddChild(o);

            return tree;
        }

        public static Command Deserialize(TreeNode<YamlObject> tree)
        {
            var allCommandTypes = AppDomain.CurrentDomain.GetAllTypesWhichImplementInterface(typeof(Command));
            var commandType = allCommandTypes.FirstOrDefault(type => type.Name.Equals(tree.value.property));

            var command = (Command)Activator.CreateInstance(commandType);

            foreach (var propNode in tree)
            {
                var field = command.GetType().GetField(propNode.value.property, k_BindingFlags);
                var fieldType = field != null ? field.FieldType : null;

                if (fieldType == null || !YamlSerializer.IsConvertibleType(fieldType))
                    continue;

                try
                {
                    var newVal = Convert.ChangeType(propNode.value.value, fieldType);
                    field.SetValue(command, newVal);
                }
                catch (Exception e)
                {
                    Logger.Log(LogType.Error, "Error while deserializing command. Failed to cast property '" + propNode.value.property + "' value to correct type", e.Message);
                }
            }

            return command;
        }
    }
}
