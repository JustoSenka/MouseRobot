using System;
using System.IO;
using System.Reflection;

namespace RobotRuntime.IO
{
    public class YamlRecordingIO : ObjectIO
    {
        private const BindingFlags k_BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        public override T LoadObject<T>(string path)
        {
            try
            {
                var text = File.ReadAllText(path);
                var yamlTree = YamlSerializer.DeserializeYamlTree(text);
                var s = Deserialize(yamlTree);
                s.Name = Path.GetFileNameWithoutExtension(path);
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
                var rec = objToWrite as LightRecording;
                rec.Name = Path.GetFileNameWithoutExtension(path);
                var yamlTree = Serialize(rec, 0);
                var text = YamlSerializer.SerializeYamlTree(yamlTree);
                File.WriteAllText(path, text);
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Failed to write to file: " + path, e.Message);
            }
        }

        public static TreeNode<YamlObject> Serialize(LightRecording recording, int level = 0)
        {
            var recordingObject = new YamlObject(level, recording.GetType().Name, "");
            var tree = new TreeNode<YamlObject>(recordingObject);

            foreach (var n in YamlSerializer.SerializeSimpleProperties(recording, level + 1))
                tree.AddChild(n);

            foreach (var node in recording.Commands)
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

        public static LightRecording Deserialize(TreeNode<YamlObject> tree)
        {
            var commandRoot = new TreeNode<Command>();

            foreach (var yamlCommandNode in tree)
                DeserializeCommandsRecursively(commandRoot, yamlCommandNode);

            var lightRecording = new LightRecording(commandRoot);
            YamlSerializer.DeserializeSimpleProperties(lightRecording, tree);

            return lightRecording;
        }

        private static void DeserializeCommandsRecursively(TreeNode<Command> commandRoot, TreeNode<YamlObject> yamlCommandNode)
        {
            var command = YamlCommandIO.Deserialize(yamlCommandNode);
            if (command == null)
                return;

            var addedCommandNode = commandRoot.AddChild(command);

            foreach (var yamlChildCommand in yamlCommandNode)
            {
                // Don't call Deserialize for non-command properties. In Yaml tree Command properties don't have any value, just property name.
                if (yamlChildCommand.value.value == "" || yamlChildCommand.value.value == string.Empty)
                    DeserializeCommandsRecursively(addedCommandNode, yamlChildCommand);
            }
        }
    }
}
