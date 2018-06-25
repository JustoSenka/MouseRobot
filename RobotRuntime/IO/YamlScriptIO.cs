using RobotRuntime.Utils;
using System;
using System.IO;
using System.Reflection;

namespace RobotRuntime.IO
{
    public class YamlScriptIO : ObjectIO
    {
        private const BindingFlags k_BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        public override T LoadObject<T>(string path)
        {
            try
            {
                var text = File.ReadAllText(path);
                var yamlTree = YamlSerializer.DeserializeYamlTree(text);
                var s = Deserialize(yamlTree);
                return (T)((object) s);
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Failed to read from file: " + path, e.Message);
            }
            return default(T);
        }

        public override void SaveObject<T>(string path, T objToWrite)
        {
            try
            {
                var yamlTree = Serialize(objToWrite as LightScript, 0);
                var text = YamlSerializer.SerializeYamlTree(yamlTree);
                File.WriteAllText(path, text);
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Failed to write to file: " + path, e.Message);
            }
        }

        public static TreeNode<YamlObject> Serialize(LightScript script, int level = 0)
        {
            var scriptObject = new YamlObject(level, script.GetType().Name, "");
            var tree = new TreeNode<YamlObject>(scriptObject);

            foreach (var n in YamlSerializer.SerializeSimpleProperties(script, level + 1))
                tree.AddChild(n);

            foreach (var node in script.Commands)
                SerializeCommandsRecursively(tree, node, level + 1);

            return tree;
        }

        private static void SerializeCommandsRecursively(TreeNode<YamlObject> parent, TreeNode<Command> commandToAdd, int level)
        {
            var commandYamlObject = YamlCommandIO.Serialize(commandToAdd.value, level);
            parent.Join(commandYamlObject);

            foreach (var childNode in commandToAdd)
                SerializeCommandsRecursively(commandYamlObject, childNode, level + 1);
        }

        public static LightScript Deserialize(TreeNode<YamlObject> tree)
        {
            var commandRoot = new TreeNode<Command>();

            foreach (var yamlCommandNode in tree)
                DeserializeCommandsRecursively(commandRoot, yamlCommandNode);

            var lightScript = new LightScript(commandRoot);
            YamlSerializer.DeserializeSimpleProperties(lightScript, tree);

            return lightScript;
        }

        private static void DeserializeCommandsRecursively(TreeNode<Command> commandRoot, TreeNode<YamlObject> yamlCommandNode)
        {
            var command = YamlCommandIO.Deserialize(yamlCommandNode);
            if (command == null)
                return;

            var addedCommandNode = commandRoot.AddChild(command);

            foreach (var yamlChildCommand in yamlCommandNode)
                DeserializeCommandsRecursively(addedCommandNode, yamlChildCommand);
        }
    }
}
