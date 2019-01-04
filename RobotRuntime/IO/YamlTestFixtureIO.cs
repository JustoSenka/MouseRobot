using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RobotRuntime.IO
{
    public class YamlTestFixtureIO : ObjectIO
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
                var yamlTree = Serialize(objToWrite as LightTestFixture);
                var text = YamlSerializer.SerializeYamlTree(yamlTree);
                File.WriteAllText(path, text);
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Failed to write to file: " + path, e.Message);
            }
        }

        public static TreeNode<YamlObject> Serialize(LightTestFixture fixture)
        {
            var level = 0;
            var fixtureObject = new YamlObject(level, fixture.GetType().Name, "");
            var tree = new TreeNode<YamlObject>(fixtureObject);

            foreach (var n in YamlSerializer.SerializeSimpleProperties(fixture, level + 1))
                tree.AddChild(n);
            
            tree.Join(YamlRecordingIO.Serialize(fixture.Setup.ToLightRecording(), level + 1));
            tree.Join(YamlRecordingIO.Serialize(fixture.TearDown.ToLightRecording(), level + 1));
            tree.Join(YamlRecordingIO.Serialize(fixture.OneTimeSetup.ToLightRecording(), level + 1));
            tree.Join(YamlRecordingIO.Serialize(fixture.OneTimeTeardown.ToLightRecording(), level + 1));

            foreach(var t in fixture.Tests)
                tree.Join(YamlRecordingIO.Serialize(t.ToLightRecording(), level + 1));

            return tree;
        }

        public static LightTestFixture Deserialize(TreeNode<YamlObject> tree)
        {
            var root = new TreeNode<LightRecording>();

            var fixture = new LightTestFixture();
            YamlSerializer.DeserializeSimpleProperties(fixture, tree);

            foreach (var yamlRecordingNode in tree)
            {
                var s = YamlRecordingIO.Deserialize(yamlRecordingNode);
                if (s == null || s.Name == null && s.Commands.Count() == 0) // Test fixture name is also part of the tree, so skip it
                    continue;

                fixture.AddRecording(Recording.FromLightRecording(s));
            }

            return fixture;
        }
    }
}
