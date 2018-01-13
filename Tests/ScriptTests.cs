using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using RobotRuntime.Perf;
using System;
using Robot.Utils;
using RobotRuntime.Utils;
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

            var script = ScriptManager.Instance.LoadedScripts[0];

            var c1 = script.AddCommand(new CommandSleep(1));
            var c2 = script.AddCommand(new CommandSleep(2));
            var c3 = script.AddCommand(new CommandSleep(3));

            script.MoveCommandAfter(c1, c3);

            Assert.AreEqual(c2, script.Commands.GetChild(0).value);
            Assert.AreEqual(c3, script.Commands.GetChild(1).value);
            Assert.AreEqual(c1, script.Commands.GetChild(2).value);
        }

        /* Those are not stable at this moment, since they change the global state of application.
         * Due to it being singleton, It cannot be reset and tested
         * 
        [TestMethod]
        public void Script_MoveCommandBefore_WorksWithinSameLevel()
        {
            MouseRobot.Instance.ForceInit();

            var script = ScriptManager.Instance.LoadedScripts[0];

            var c1 = script.AddCommand(new CommandSleep(1));
            var c2 = script.AddCommand(new CommandSleep(2));
            var c3 = script.AddCommand(new CommandSleep(3));

            script.MoveCommandBefore(c1, c3);

            Assert.AreEqual(c2, script.Commands.GetChild(0).value);
            Assert.AreEqual(c1, script.Commands.GetChild(1).value);
            Assert.AreEqual(c3, script.Commands.GetChild(2).value);
        }

        [TestMethod]
        public void Script_MoveCommand_MultipleTimes_WorksWithinSameLevel()
        {
            MouseRobot.Instance.ForceInit();

            var script = ScriptManager.Instance.LoadedScripts[0];

            var c1 = script.AddCommand(new CommandSleep(1));
            var c2 = script.AddCommand(new CommandSleep(2));
            var c3 = script.AddCommand(new CommandSleep(3));

            script.MoveCommandAfter(c1, c3);
            script.MoveCommandBefore(c2, c1);
            script.MoveCommandAfter(c2, c3);
            script.MoveCommandBefore(c1, c2);
            script.MoveCommandAfter(c3, c1);
            script.MoveCommandBefore(c2, c3);
            script.MoveCommandAfter(c1, c2);
            script.MoveCommandBefore(c3, c1);

            Assert.AreEqual(c2, script.Commands.GetChild(0).value);
            Assert.AreEqual(c3, script.Commands.GetChild(1).value);
            Assert.AreEqual(c1, script.Commands.GetChild(2).value);
        }*/
    }
}
