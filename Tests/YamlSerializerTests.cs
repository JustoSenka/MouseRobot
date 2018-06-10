using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot.Scripts;
using RobotRuntime;
using RobotRuntime.Commands;
using RobotRuntime.IO;
using System;
using System.Linq;
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
        public void Command_ProducesCorrect_YamlString()
        {
            var yamlObj = YamlCommandIO.Serialize(command, 0);
            var yamlString = YamlSerializer.SerializeYamlTree(yamlObj);
            Assert.AreEqual(serializedCommand, yamlString, "Strings missmatched.");
        }

        [TestMethod]
        public void Command_ProducesCorrect_YamlObject()
        {
            var yamlObj = YamlCommandIO.Serialize(command, 0);

            var props = yamlObj.ToArray();
            Assert.AreEqual("CommandPress", yamlObj.value.property, "Command object had incorrect command type indicator.");
            Assert.AreEqual(3, props.Length, "Command object should have 3 properties.");
        }

        [TestMethod]
        public void YamlObject_ProducesCorrect_Command()
        {
            var yamlObj = YamlCommandIO.Serialize(command, 0);
            var newCommand = YamlCommandIO.Deserialize(yamlObj);
            Assert.AreEqual(command.ToString(), newCommand.ToString(), "Command strings should be equal.");
        }

        [TestMethod]
        public void YamlString_ProducesCorrect_CommandYamlObject()
        {
            var tree = YamlSerializer.DeserializeYamlTree(serializedCommand);
            var props = tree.ToArray();

            Assert.AreEqual("CommandPress", tree.value.property, "Command object had incorrect command type indicator.");
            Assert.AreEqual(3, props.Length, "Command object should have 3 properties.");
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
        public void Script_ProducesCorrect_YamlString()
        {
            var yamlObj = YamlScriptIO.Serialize(Script);
            var yamlString = YamlSerializer.SerializeYamlTree(yamlObj);
            StringAssert.Contains(serializedScript, yamlString, "Strings missmatched.");
        }

        [TestMethod]
        public void Script_ProducesCorrect_YamlObj()
        {
            var yamlObj = YamlScriptIO.Serialize(Script);
            var commands = yamlObj.ToArray();
            Assert.AreEqual(2, commands.Length, "only two root commands should be in the script.");
            Assert.AreEqual(2, commands[0].ToArray().Length, "Image command has also two childs, timeout and CommandPress.");
            Assert.AreEqual(2, commands[1].ToArray().Length, "Command move has also two childs, X and Y.");

            var commandPress = commands[0].ToArray()[1];
            Assert.AreEqual("CommandPress", commandPress.value.property, "CommandPress value of YamlObject was incorrect");
            Assert.AreEqual(3, commandPress.ToArray().Length, "CommandPress has also three childs, X Y DontMove");
        }

        [TestMethod]
        public void YamlString_ProducesCorrect_ScriptYamlObject()
        {
            var yamlObj = YamlSerializer.DeserializeYamlTree(serializedScript);

            var commands = yamlObj.ToArray();
            Assert.AreEqual(2, commands.Length, "only two root commands should be in the script.");
            Assert.AreEqual(2, commands[0].ToArray().Length, "Image command has also two childs, timeout and CommandPress.");
            Assert.AreEqual(2, commands[1].ToArray().Length, "Command move has also two childs, X and Y.");

            var commandPress = commands[0].ToArray()[1];
            Assert.AreEqual("CommandPress", commandPress.value.property, "CommandPress value of YamlObject was incorrect");
            Assert.AreEqual(3, commandPress.ToArray().Length, "CommandPress has also three childs, X Y DontMove");
        }

        [TestMethod]
        public void YamlObject_ProducesCorrect_Script()
        {
            var s = Script;
            var lightScript = s.ToLightScript();
            var yamlObj = YamlScriptIO.Serialize(s);
            var newScript = YamlScriptIO.Deserialize(yamlObj);

            Assert.AreEqual(lightScript.Commands.Count(), newScript.Commands.Count(), "Command count should be the same.");
        }

        [TestInitialize]
        public void InitializeTest()
        {
            Logger.Instance = new FakeLogger();
        }
    }
}
