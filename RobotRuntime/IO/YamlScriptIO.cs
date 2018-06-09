using RobotRuntime.Utils;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using YamlDotNet.Serialization;

namespace RobotRuntime.IO
{
    public class YamlScriptIO : ObjectIO
    {
        private const BindingFlags k_BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        public override T LoadObject<T>(string path)
        {
            return new BinaryObjectIO().LoadObject<T>(path);

            try
            {
                var text = File.ReadAllText(path);
                var deserializer = new DeserializerBuilder().Build();
                return deserializer.Deserialize<T>(text);
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Failed to read from file: " + path, e.Message);
            }
            return default(T);
        }

        public override void SaveObject<T>(string path, T objToWrite)
        {
            /* try
             {*/
            var builder = new StringBuilder();

            //YamlSerializer.WriteAllPropertiesRecursivelly(builder, objToWrite, 0);

            File.WriteAllText(path, builder.ToString());
            /*}
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Failed to write to file: " + path, e.Message);
            }*/
        }

        public static TreeNode<YamlObject> Serialize(LightScript script)
        {
            var level = 0;
            var scriptObject = new YamlObject(level, script.GetType().Name, "");
            var tree = new TreeNode<YamlObject>(scriptObject);

            foreach(var node in script.Commands)
                AddCommandsRecursively(tree, node, level + 1);

            return tree;
        }

        private static void AddCommandsRecursively(TreeNode<YamlObject> parent, TreeNode<Command> commandToAdd, int level)
        {
            parent.Join(YamlCommandIO.Serialize(commandToAdd.value, level));

            foreach (var childNode in commandToAdd)
                AddCommandsRecursively(parent, childNode, level + 1);
        }
    }
}
