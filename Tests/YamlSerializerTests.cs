using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot.Scripts;
using RobotRuntime;
using RobotRuntime.Commands;
using RobotRuntime.IO;
using System;
using Tests.Fakes;

namespace Tests
{
    [TestClass]
    public class YamlSerializerTests
    {
        private short level = 2;
        private string PropertyName = "SomeInt";
        private int PropertyValue = 157;
        private string separator = ": ";

        private string YamlLine { get { return YamlObject.GetIndentation(level) + PropertyName + separator + PropertyValue; } }

        [TestMethod]
        public void YamlObject_ProducesCorrect_YamlString()
        {
            var obj = new YamlObject(level, PropertyName, PropertyValue);
            Assert.AreEqual(YamlLine, obj.ToString(), "Yaml Lines should be identical");
        }

        [TestMethod]
        public void YamlString_ProducesCorrect_YamlObject()
        {
            var obj = new YamlObject(YamlLine);

            Assert.AreEqual(level, obj.level, "Level missmatched");
            Assert.AreEqual(PropertyName, obj.property, "Property name missmatched");
            Assert.AreEqual(PropertyValue.ToString(), obj.value, "Property value missmatched");
        }



        private Command command = new CommandPress(50, 70, false);
        private string serializedCommand = @"CommandPress: 
  <X>k__BackingField: 50
  <Y>k__BackingField: 70
  <DontMove>k__BackingField: False";

        [TestMethod]
        public void CommandToYaml_ProducesCorrect_YamlString()
        {
            var yamlObj = YamlCommandIO.Serialize(command, 0);
            var yamlString = YamlSerializer.SerializeYamlTree(yamlObj);
            Assert.AreEqual(serializedCommand, yamlString, "Strings missmatched.");
        }

        [TestMethod]
        public void YamlObjectToCommand_ProducesCorrect_Command()
        {
            var yamlObj = YamlCommandIO.Serialize(command, 0);
            var newCommand = YamlCommandIO.Deserialize(yamlObj);
            Assert.AreEqual(command.ToString(), newCommand.ToString(), "Command strings should be equal.");
        }


        private Script Script { get
            {
                var s = new Script();
                s.ScriptManager = new FakeScriptManager();
                var imageCommand = new CommandForImage(new Guid(), 1850);
                s.AddCommand(imageCommand);
                s.AddCommand(new CommandPress(55, 66, true), imageCommand);
                s.AddCommand(new CommandMove(10, 20));
                return s;
            }
        }
        private string serializedScript = @"Script: 
  CommandForImage: 
    <Timeout>k__BackingField: 1850
    CommandPress: 
      <X>k__BackingField: 55
      <Y>k__BackingField: 66
      <DontMove>k__BackingField: True
  CommandMove: 
    <X>k__BackingField: 10
    <Y>k__BackingField: 20";

        [TestMethod]
        public void ScriptToYaml_ProducesCorrect_YamlString()
        {
            var yamlObj = YamlScriptIO.Serialize(Script);
            var yamlString = YamlSerializer.SerializeYamlTree(yamlObj);
            StringAssert.Contains(serializedScript, yamlString, "Strings missmatched.");
        }

        [TestInitialize]
        public void InitializeTest()
        {
            Logger.Instance = new FakeLogger();
        }
    }
}
