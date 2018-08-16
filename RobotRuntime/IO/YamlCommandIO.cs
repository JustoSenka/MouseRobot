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

            tree.AddChild(new YamlObject(level + 1, "<Guid>k__BackingField", command.Guid));

            var objs = YamlSerializer.SerializeSimpleProperties(command, level + 1);

            foreach (var o in objs)
                tree.AddChild(o);

            return tree;
        }

        public static Command Deserialize(TreeNode<YamlObject> tree)
        {
            // TODO: These two lines take a lot of time, and they will be called a lot of times!!!
            var allCommandTypes = AppDomain.CurrentDomain.GetAllTypesWhichImplementInterface(typeof(Command));
            var commandType = allCommandTypes.FirstOrDefault(type => type.Name.Equals(tree.value.property));

            if (commandType == null)
                return null;

            var command = (Command)Activator.CreateInstance(commandType);
            YamlSerializer.DeserializeSimpleProperties(command, tree);

            return command;
        }
    }
}
