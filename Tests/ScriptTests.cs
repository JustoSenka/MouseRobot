using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Robot;
using RobotRuntime.Commands;

namespace Tests
{
    [TestClass]
    public class ScriptTests
    {
        [TestMethod]
        public void NewlyCreatedScriptManager_WillHaveOneScriptOpen()
        {
            MouseRobot.Instance.ForceInit();
            Assert.AreEqual(1, ScriptManager.Instance.LoadedScripts.Count);
        }

        [TestMethod]
        public void Script_MoveCommandAfter_WorksWithinSameLevel()
        {
            MouseRobot.Instance.ForceInit();
            var s = ScriptManager.Instance.LoadedScripts[0];

            var c1 = s.AddCommand(new CommandSleep(1));
            var c2 = s.AddCommand(new CommandSleep(2));
            var c3 = s.AddCommand(new CommandSleep(3));

            s.MoveCommandAfter(c1, c3);

            Assert.AreEqual(c2, s.Commands.GetChild(0).value);
            Assert.AreEqual(c3, s.Commands.GetChild(1).value);
            Assert.AreEqual(c1, s.Commands.GetChild(2).value);

            ResetScriptManager();
        }


        [TestMethod]
        public void Script_MoveCommandBefore_WorksWithinSameLevel()
        {
            MouseRobot.Instance.ForceInit();
            var s = ScriptManager.Instance.LoadedScripts[0];

            var c1 = s.AddCommand(new CommandSleep(1));
            var c2 = s.AddCommand(new CommandSleep(2));
            var c3 = s.AddCommand(new CommandSleep(3));

            s.MoveCommandBefore(c1, c3);

            Assert.AreEqual(c2, s.Commands.GetChild(0).value);
            Assert.AreEqual(c1, s.Commands.GetChild(1).value);
            Assert.AreEqual(c3, s.Commands.GetChild(2).value);

            ResetScriptManager();
        }

        [TestMethod]
        public void Script_MoveCommand_MultipleTimes_WorksWithinSameLevel()
        {
            MouseRobot.Instance.ForceInit();
            var s = ScriptManager.Instance.LoadedScripts[0];

            var c1 = s.AddCommand(new CommandSleep(1));
            var c2 = s.AddCommand(new CommandSleep(2));
            var c3 = s.AddCommand(new CommandSleep(3));

            s.MoveCommandAfter(c1, c3);
            s.MoveCommandBefore(c2, c1);
            s.MoveCommandAfter(c2, c3);
            s.MoveCommandBefore(c1, c2);
            s.MoveCommandAfter(c3, c1);
            s.MoveCommandBefore(c2, c3);
            s.MoveCommandAfter(c1, c2);
            s.MoveCommandBefore(c3, c1);

            Assert.AreEqual(c2, s.Commands.GetChild(0).value);
            Assert.AreEqual(c3, s.Commands.GetChild(1).value);
            Assert.AreEqual(c1, s.Commands.GetChild(2).value);

            ResetScriptManager();
        }

        [TestMethod]
        public void Script_MoveCommandAfter_WorksToUpperLevel()
        {
            MouseRobot.Instance.ForceInit();
            var s = ScriptManager.Instance.LoadedScripts[0];

            var c1 = s.AddCommand(new CommandSleep(1));
            var c2 = s.AddCommand(new CommandSleep(2));
            var c3 = s.AddCommand(new CommandSleep(3));

            var n1 = s.Commands.GetNodeFromValue(c1);
            var c11 = s.AddCommand(new CommandSleep(4), c1);

            s.MoveCommandAfter(c3, c11);

            Assert.AreEqual(2, s.Commands.Count());
            Assert.AreEqual(2, n1.Count());
            Assert.AreEqual(c11, n1.GetChild(0).value);
            Assert.AreEqual(c3, n1.GetChild(1).value);

            ResetScriptManager();
        }

        [TestMethod]
        public void Script_MoveCommandBefore_WorksToUpperLevel()
        {
            MouseRobot.Instance.ForceInit();
            var s = ScriptManager.Instance.LoadedScripts[0];

            var c1 = s.AddCommand(new CommandSleep(1));
            var c2 = s.AddCommand(new CommandSleep(2));
            var c3 = s.AddCommand(new CommandSleep(3));

            var n1 = s.Commands.GetNodeFromValue(c1);
            var c11 = s.AddCommand(new CommandSleep(4), c1);

            s.MoveCommandBefore(c3, c11);

            Assert.AreEqual(2, s.Commands.Count());
            Assert.AreEqual(2, n1.Count());
            Assert.AreEqual(c3, n1.GetChild(0).value);
            Assert.AreEqual(c11, n1.GetChild(1).value);

            ResetScriptManager();
        }

        [TestMethod]
        public void Script_MoveCommandAfter_WorksToLowerLevel()
        {
            MouseRobot.Instance.ForceInit();
            var s = ScriptManager.Instance.LoadedScripts[0];

            var c1 = s.AddCommand(new CommandSleep(1));

            var n1 = s.Commands.GetNodeFromValue(c1);
            var c11 = s.AddCommand(new CommandSleep(2), c1);
            var c12 = s.AddCommand(new CommandSleep(3), c1);

            s.MoveCommandAfter(c12, c1);

            Assert.AreEqual(2, s.Commands.Count());
            Assert.AreEqual(1, n1.Count());
            Assert.AreEqual(c1, s.Commands.GetChild(0).value);
            Assert.AreEqual(c12, s.Commands.GetChild(1).value);

            ResetScriptManager();
        }

        [TestMethod]
        public void Script_MoveCommandBefore_WorksToLowerLevel()
        {
            MouseRobot.Instance.ForceInit();
            var s = ScriptManager.Instance.LoadedScripts[0];

            var c1 = s.AddCommand(new CommandSleep(1));

            var n1 = s.Commands.GetNodeFromValue(c1);
            var c11 = s.AddCommand(new CommandSleep(2), c1);
            var c12 = s.AddCommand(new CommandSleep(3), c1);

            s.MoveCommandBefore(c12, c1);

            Assert.AreEqual(2, s.Commands.Count());
            Assert.AreEqual(1, n1.Count());
            Assert.AreEqual(c12, s.Commands.GetChild(0).value);
            Assert.AreEqual(c1, s.Commands.GetChild(1).value);

            ResetScriptManager();
        }

        [TestMethod]
        public void Script_MoveNestedCommands_MovesAllChildCommandsAlso()
        {
            MouseRobot.Instance.ForceInit();
            var s = ScriptManager.Instance.LoadedScripts[0];

            var c1 = s.AddCommand(new CommandSleep(1));
            var c11 = s.AddCommand(new CommandSleep(2), c1);
            var c2 = s.AddCommand(new CommandSleep(3));
            var c22 = s.AddCommand(new CommandSleep(4), c2);

            s.MoveCommandBefore(c2, c11);
            var n2 = s.Commands.GetNodeFromValue(c2);

            Assert.AreEqual(1, s.Commands.Count());
            Assert.AreEqual(2, s.Commands.GetChild(0).Count());
            Assert.AreEqual(1, n2.Count());

            Assert.AreEqual(c22, n2.GetChild(0).value);

            ResetScriptManager();
        }

        [TestMethod]
        public void Script_InsertCommand_InsertsCommandInCorrectPosition()
        {
            MouseRobot.Instance.ForceInit();
            var s = ScriptManager.Instance.LoadedScripts[0];

            var c2 = s.AddCommand(new CommandSleep(2));
            var c4 = s.AddCommand(new CommandSleep(4));

            var c1 = new CommandSleep(1);
            var c3 = new CommandSleep(3);
            var c5 = new CommandSleep(5);

            s.InsertCommand(c1, 0);
            s.InsertCommandAfter(c3, c2);
            s.InsertCommand(c5, 4);

            Assert.AreEqual(c1, s.Commands.GetChild(0).value);
            Assert.AreEqual(c2, s.Commands.GetChild(1).value);
            Assert.AreEqual(c3, s.Commands.GetChild(2).value);
            Assert.AreEqual(c4, s.Commands.GetChild(3).value);
            Assert.AreEqual(c5, s.Commands.GetChild(4).value);

            ResetScriptManager();
        }

        [TestMethod]
        public void Script_AddCommandNode_AddAllCommandsWithIt()
        {
            MouseRobot.Instance.ForceInit();
            var s = ScriptManager.Instance.LoadedScripts[0];

            var c1 = s.AddCommand(new CommandSleep(1));
            var c2 = s.AddCommand(new CommandSleep(3));
            var c22 = s.AddCommand(new CommandSleep(4), c2);

            var n2 = s.Commands.GetNodeFromValue(c2);

            s.RemoveCommand(c2);
            s.AddCommandNode(n2, c1);

            Assert.AreEqual(1, s.Commands.Count());
            Assert.AreEqual(1, s.Commands.GetChild(0).Count());
            Assert.AreEqual(1, s.Commands.GetChild(0).GetChild(0).Count());
            Assert.AreEqual(c2, s.Commands.GetChild(0).GetChild(0).value);
            Assert.AreEqual(c22, s.Commands.GetChild(0).GetChild(0).GetChild(0).value);

            ResetScriptManager();
        }

        [TestMethod]
        public void Script_RemoveCommand_RemovesItWithChildren()
        {
            MouseRobot.Instance.ForceInit();
            var s = ScriptManager.Instance.LoadedScripts[0];

            var c1 = s.AddCommand(new CommandSleep(1));
            var c2 = s.AddCommand(new CommandSleep(2));
            var c22 = s.AddCommand(new CommandSleep(3), c2);
            var c3 = s.AddCommand(new CommandSleep(4));

            s.RemoveCommand(c2);

            Assert.AreEqual(2, s.Commands.Count());
            Assert.AreEqual(c1, s.Commands.GetChild(0).value);
            Assert.AreEqual(c3, s.Commands.GetChild(1).value);

            Assert.IsFalse(s.Select(n => n.value).Contains(c2));
            Assert.IsFalse(s.Select(n => n.value).Contains(c22));

            ResetScriptManager();
        }

        private void ResetScriptManager()
        {
            for (int i = ScriptManager.Instance.LoadedScripts.Count - 1; i >= 0; --i)
            {
                ScriptManager.Instance.RemoveScript(ScriptManager.Instance.LoadedScripts[i]);
            }
            ScriptManager.Instance.NewScript();
        }
    }
}
