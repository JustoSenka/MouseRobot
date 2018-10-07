using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Commands;
using RobotRuntime.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity;

namespace Tests
{
    [TestClass]
    public class ScriptManagerTests
    {
        IScriptManager ScriptManager;

        [TestMethod]
        public void NewlyCreatedScriptManager_WillHaveOneScriptOpen()
        {
            Assert.AreEqual(1, ScriptManager.LoadedScripts.Count);
        }

        [TestMethod]
        public void ScriptManager_NewScript_WillCreateSecondEmptyScript()
        {
            ScriptManager.NewScript();
            Assert.AreEqual(2, ScriptManager.LoadedScripts.Count);
        }

        private Script NewTestScript(out Command topCommand, out Command childCommand)
        {
            var s = ScriptManager.NewScript();
            topCommand = s.AddCommand(new CommandSleep(1));
            childCommand = s.AddCommand(new CommandSleep(2), topCommand);
            return s;
        }

        [TestMethod]
        public void ScriptManager_MoveCommandAfter_ToOtherScript_MovesFullNode()
        {
            var s1 = NewTestScript(out Command c1, out Command c11);
            var s2 = NewTestScript(out Command c2, out Command c21);

            ScriptManager.MoveCommandAfter(c1, c2, 1, 2);

            Assert.AreEqual(0, s1.Commands.Count());
            Assert.AreEqual(2, s2.Commands.Count());
            Assert.AreEqual(c1, s2.Commands.GetChild(1).value);
            Assert.AreEqual(c11, s2.Commands.GetChild(1).GetChild(0).value);
        }

        [TestMethod]
        public void ScriptManager_MoveCommandBefore_ToOtherScript_MovesFullNode()
        {
            var s1 = NewTestScript(out Command c1, out Command c11);
            var s2 = NewTestScript(out Command c2, out Command c21);

            ScriptManager.MoveCommandBefore(c1, c2, 1, 2);

            Assert.AreEqual(0, s1.Commands.Count());
            Assert.AreEqual(2, s2.Commands.Count());
            Assert.AreEqual(c1, s2.Commands.GetChild(0).value);
            Assert.AreEqual(c11, s2.Commands.GetChild(0).GetChild(0).value);
        }

        [TestMethod]
        public void ScriptManager_MoveCommandAfter_ToOtherScriptButNotRoot_AlsoWorks()
        {
            var s1 = NewTestScript(out Command c1, out Command c11);
            var s2 = NewTestScript(out Command c2, out Command c21);

            ScriptManager.MoveCommandAfter(c1, c21, 1, 2);

            Assert.AreEqual(0, s1.Commands.Count());
            Assert.AreEqual(1, s2.Commands.Count());
            Assert.AreEqual(c1, s2.Commands.GetChild(0).GetChild(1).value);
            Assert.AreEqual(c11, s2.Commands.GetChild(0).GetChild(1).GetChild(0).value);
        }

        [TestMethod]
        public void ScriptManager_MoveCommandBefore_ToOtherScriptButNotRoot_AlsoWorks()
        {
            var s1 = NewTestScript(out Command c1, out Command c11);
            var s2 = NewTestScript(out Command c2, out Command c21);

            ScriptManager.MoveCommandBefore(c1, c21, 1, 2);

            Assert.AreEqual(0, s1.Commands.Count());
            Assert.AreEqual(1, s2.Commands.Count());
            Assert.AreEqual(c1, s2.Commands.GetChild(0).GetChild(0).value);
            Assert.AreEqual(c11, s2.Commands.GetChild(0).GetChild(0).GetChild(0).value);
        }

        [TestMethod]
        public void ScriptManager_MoveCommandAfter_FromChildToRoot_AlsoWorks()
        {
            var s1 = NewTestScript(out Command c1, out Command c11);
            var s2 = NewTestScript(out Command c2, out Command c21);

            ScriptManager.MoveCommandAfter(c11, c2, 1, 2);

            Assert.AreEqual(1, s1.Commands.Count());
            Assert.AreEqual(2, s2.Commands.Count());
            Assert.AreEqual(c2, s2.Commands.GetChild(0).value);
            Assert.AreEqual(c11, s2.Commands.GetChild(1).value);
        }

        [TestMethod]
        public void ScriptManager_MoveCommandBefore_FromChildToRoot_AlsoWorks()
        {
            var s1 = NewTestScript(out Command c1, out Command c11);
            var s2 = NewTestScript(out Command c2, out Command c21);

            ScriptManager.MoveCommandBefore(c11, c2, 1, 2);

            Assert.AreEqual(1, s1.Commands.Count());
            Assert.AreEqual(2, s2.Commands.Count());
            Assert.AreEqual(c2, s2.Commands.GetChild(1).value);
            Assert.AreEqual(c11, s2.Commands.GetChild(0).value);
        }


        [TestMethod]
        public void ScriptManager_MoveCommandAfter_SameScript()
        {
            var s1 = NewTestScript(out Command c1, out Command c11);

            ScriptManager.MoveCommandAfter(c11, c1, 1);

            Assert.AreEqual(2, s1.Commands.Count());
            Assert.AreEqual(c1, s1.Commands.GetChild(0).value);
            Assert.AreEqual(c11, s1.Commands.GetChild(1).value);
        }

        [TestMethod]
        public void ScriptManager_MoveCommandBefore_SameScript()
        {
            var s1 = NewTestScript(out Command c1, out Command c11);

            ScriptManager.MoveCommandBefore(c11, c1, 1);

            Assert.AreEqual(2, s1.Commands.Count());
            Assert.AreEqual(c1, s1.Commands.GetChild(1).value);
            Assert.AreEqual(c11, s1.Commands.GetChild(0).value);
        }

        [TestMethod]
        public void Scripts_HaveCorrectGuids_AfterMovingCommandToOtherScript()
        {
            var s1 = NewTestScript(out Command c1, out Command c11);
            var s2 = NewTestScript(out Command c2, out Command c22);

            ScriptManager.MoveCommandBefore(c2, c1, 2, 1);

            CheckIfScriptsHasAllCorrectGuids(s1, s2);
        }

        [TestMethod]
        public void Scrips_HaveCorrectGuids_AfterDuplicatingCommand()
        {
            var s = NewTestScript(out Command c1, out Command c11);

            var clone = s.CloneCommandStub(c1);
            s.AddCommandNode(clone);

            CheckIfScriptsHasAllCorrectGuids(s);
        }

        [TestMethod]
        public void Scripts_HaveCorrectGuids_AfterDuplicatingCommand_AndMovingBackAndForthBetweenScripts()
        {
            var s1 = NewTestScript(out Command c1, out Command c11);
            var s2 = NewTestScript(out Command c2, out Command c22);

            var clone = s1.CloneCommandStub(c1);
            var command = s1.AddCommandNode(clone);

            ScriptManager.MoveCommandBefore(command, c2, 1, 2);
            ScriptManager.MoveCommandBefore(command, c1, 2, 1);

            CheckIfScriptsHasAllCorrectGuids(s1, s2);
        }

        [TestMethod]
        public void Scripts_HaveCorrectGuids_AfterDuplicatingScript()
        {
            var s1 = NewTestScript(out Command c1, out Command c11);
            var s2 = ScriptManager.NewScript(s1);

            CheckIfScriptsHasAllCorrectGuids(s1, s2);
        }

        [TestMethod]
        public void Scripts_HaveCorrectGuids_AfterDuplicatingScript_AndMovingCommandsToIt()
        {
            var s1 = NewTestScript(out Command c1, out Command c11);
            var s2 = ScriptManager.NewScript(s1);

            var node = s1.Commands.GetNodeFromValue(c1);
            s2.AddCommandNode(node);

            CheckIfScriptsHasAllCorrectGuids(s1, s2);
        }

        [TestMethod]
        public void ScriptManager_CloneNewScript_RegeneratesCommandAndScriptGuids()
        {
            var s1 = NewTestScript(out Command c1, out Command c11);
            var s2 = ScriptManager.NewScript(s1);

            TestBase.CheckThatGuidsAreNotSame(s1, s2);
            TestBase.CheckThatPtrsAreNotSame(s1, s2);
        }

        private void CheckIfScriptsHasAllCorrectGuids(params Script[] scripts)
        {
            var hashmapField = typeof(Script).GetField("CommandGuidMap", BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var s in scripts)
            {
                var commands = s.Commands.GetAllNodes(false).Select(node => node.value);
                var hashmap = (HashSet<Guid>)hashmapField.GetValue(s);

                Assert.AreEqual(commands.Count(), hashmap.Count, $"Hashmap guid count missmatched with command count: {s.Name}");
                foreach (var c in commands)
                    Assert.IsTrue(s.HasRegisteredGuid(c.Guid), $"Command {c.Name} is not registered in script {s.Name}");
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            var container = TestBase.ConstructContainerForTests();
            var mr = container.Resolve<IMouseRobot>();
            ScriptManager = container.Resolve<IScriptManager>();
        }

        [TestCleanup]
        public void ResetScriptManager()
        {
            for (int i = ScriptManager.LoadedScripts.Count - 1; i >= 0; --i)
            {
                ScriptManager.RemoveScript(ScriptManager.LoadedScripts[i]);
            }
            ScriptManager.NewScript();
        }
    }
}
