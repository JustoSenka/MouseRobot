using NUnit.Framework;
using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Commands;
using RobotRuntime.Recordings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity;

namespace Tests.Unit
{
    [TestFixture]
    public class HierarchyManagerTests
    {
        IHierarchyManager RecordingManager;

        [SetUp]
        public void Initialize()
        {
            var container = TestBase.ConstructContainerForTests();
            var mr = container.Resolve<IMouseRobot>();
            RecordingManager = container.Resolve<IHierarchyManager>();
        }

        [Test]
        public void NewlyCreatedRecordingManager_WillHaveOneRecordingOpen()
        {
            Assert.AreEqual(1, RecordingManager.LoadedRecordings.Count);
        }

        [Test]
        public void RecordingManager_NewRecording_WillCreateSecondEmptyRecording()
        {
            RecordingManager.NewRecording();
            Assert.AreEqual(2, RecordingManager.LoadedRecordings.Count);
        }

        private Recording NewTestRecording(out Command topCommand, out Command childCommand)
        {
            var s = RecordingManager.NewRecording();
            topCommand = s.AddCommand(new CommandSleep(1));
            childCommand = s.AddCommand(new CommandSleep(2), topCommand);
            return s;
        }

        [Test]
        public void RecordingManager_MoveCommandAfter_ToOtherRecording_MovesFullNode()
        {
            var s1 = NewTestRecording(out Command c1, out Command c11);
            var s2 = NewTestRecording(out Command c2, out Command c21);

            RecordingManager.MoveCommandAfter(c1, c2, 1, 2);

            Assert.AreEqual(0, s1.Commands.Count());
            Assert.AreEqual(2, s2.Commands.Count());
            Assert.AreEqual(c1, s2.Commands.GetChild(1).value);
            Assert.AreEqual(c11, s2.Commands.GetChild(1).GetChild(0).value);
        }

        [Test]
        public void RecordingManager_MoveCommandBefore_ToOtherRecording_MovesFullNode()
        {
            var s1 = NewTestRecording(out Command c1, out Command c11);
            var s2 = NewTestRecording(out Command c2, out Command c21);

            RecordingManager.MoveCommandBefore(c1, c2, 1, 2);

            Assert.AreEqual(0, s1.Commands.Count());
            Assert.AreEqual(2, s2.Commands.Count());
            Assert.AreEqual(c1, s2.Commands.GetChild(0).value);
            Assert.AreEqual(c11, s2.Commands.GetChild(0).GetChild(0).value);
        }

        [Test]
        public void RecordingManager_MoveCommandAfter_ToOtherRecordingButNotRoot_AlsoWorks()
        {
            var s1 = NewTestRecording(out Command c1, out Command c11);
            var s2 = NewTestRecording(out Command c2, out Command c21);

            RecordingManager.MoveCommandAfter(c1, c21, 1, 2);

            Assert.AreEqual(0, s1.Commands.Count());
            Assert.AreEqual(1, s2.Commands.Count());
            Assert.AreEqual(c1, s2.Commands.GetChild(0).GetChild(1).value);
            Assert.AreEqual(c11, s2.Commands.GetChild(0).GetChild(1).GetChild(0).value);
        }

        [Test]
        public void RecordingManager_MoveCommandBefore_ToOtherRecordingButNotRoot_AlsoWorks()
        {
            var s1 = NewTestRecording(out Command c1, out Command c11);
            var s2 = NewTestRecording(out Command c2, out Command c21);

            RecordingManager.MoveCommandBefore(c1, c21, 1, 2);

            Assert.AreEqual(0, s1.Commands.Count());
            Assert.AreEqual(1, s2.Commands.Count());
            Assert.AreEqual(c1, s2.Commands.GetChild(0).GetChild(0).value);
            Assert.AreEqual(c11, s2.Commands.GetChild(0).GetChild(0).GetChild(0).value);
        }

        [Test]
        public void RecordingManager_MoveCommandAfter_FromChildToRoot_AlsoWorks()
        {
            var s1 = NewTestRecording(out Command c1, out Command c11);
            var s2 = NewTestRecording(out Command c2, out Command c21);

            RecordingManager.MoveCommandAfter(c11, c2, 1, 2);

            Assert.AreEqual(1, s1.Commands.Count());
            Assert.AreEqual(2, s2.Commands.Count());
            Assert.AreEqual(c2, s2.Commands.GetChild(0).value);
            Assert.AreEqual(c11, s2.Commands.GetChild(1).value);
        }

        [Test]
        public void RecordingManager_MoveCommandBefore_FromChildToRoot_AlsoWorks()
        {
            var s1 = NewTestRecording(out Command c1, out Command c11);
            var s2 = NewTestRecording(out Command c2, out Command c21);

            RecordingManager.MoveCommandBefore(c11, c2, 1, 2);

            Assert.AreEqual(1, s1.Commands.Count());
            Assert.AreEqual(2, s2.Commands.Count());
            Assert.AreEqual(c2, s2.Commands.GetChild(1).value);
            Assert.AreEqual(c11, s2.Commands.GetChild(0).value);
        }


        [Test]
        public void RecordingManager_MoveCommandAfter_SameRecording()
        {
            var s1 = NewTestRecording(out Command c1, out Command c11);

            RecordingManager.MoveCommandAfter(c11, c1, 1);

            Assert.AreEqual(2, s1.Commands.Count());
            Assert.AreEqual(c1, s1.Commands.GetChild(0).value);
            Assert.AreEqual(c11, s1.Commands.GetChild(1).value);
        }

        [Test]
        public void RecordingManager_MoveCommandBefore_SameRecording()
        {
            var s1 = NewTestRecording(out Command c1, out Command c11);

            RecordingManager.MoveCommandBefore(c11, c1, 1);

            Assert.AreEqual(2, s1.Commands.Count());
            Assert.AreEqual(c1, s1.Commands.GetChild(1).value);
            Assert.AreEqual(c11, s1.Commands.GetChild(0).value);
        }

        [Test]
        public void Recordings_HaveCorrectGuids_AfterMovingCommandToOtherRecording()
        {
            var s1 = NewTestRecording(out Command c1, out Command c11);
            var s2 = NewTestRecording(out Command c2, out Command c22);

            RecordingManager.MoveCommandBefore(c2, c1, 2, 1);

            CheckIfRecordingsHasAllCorrectGuids(s1, s2);
        }

        [Test]
        public void Scrips_HaveCorrectGuids_AfterDuplicatingCommand()
        {
            var s = NewTestRecording(out Command c1, out Command c11);

            var clone = s.CloneCommandStub(c1);
            s.AddCommandNode(clone);

            CheckIfRecordingsHasAllCorrectGuids(s);
        }

        [Test]
        public void Recordings_HaveCorrectGuids_AfterDuplicatingCommand_AndMovingBackAndForthBetweenRecordings()
        {
            var s1 = NewTestRecording(out Command c1, out Command c11);
            var s2 = NewTestRecording(out Command c2, out Command c22);

            var clone = s1.CloneCommandStub(c1);
            var command = s1.AddCommandNode(clone);

            RecordingManager.MoveCommandBefore(command, c2, 1, 2);
            RecordingManager.MoveCommandBefore(command, c1, 2, 1);

            CheckIfRecordingsHasAllCorrectGuids(s1, s2);
        }

        [Test]
        public void Recordings_HaveCorrectGuids_AfterDuplicatingRecording()
        {
            var s1 = NewTestRecording(out Command c1, out Command c11);
            var s2 = RecordingManager.NewRecording(s1);

            CheckIfRecordingsHasAllCorrectGuids(s1, s2);
        }

        [Test]
        public void Recordings_HaveCorrectGuids_AfterDuplicatingRecording_AndMovingCommandsToIt()
        {
            var s1 = NewTestRecording(out Command c1, out Command c11);
            var s2 = RecordingManager.NewRecording(s1);

            var node = s1.Commands.GetNodeFromValue(c1);
            s2.AddCommandNode(node);

            CheckIfRecordingsHasAllCorrectGuids(s1, s2);
        }

        [Test]
        public void RecordingManager_CloneNewRecording_RegeneratesCommandAndRecordingGuids()
        {
            var s1 = NewTestRecording(out Command c1, out Command c11);
            var s2 = RecordingManager.NewRecording(s1);

            TestBase.CheckThatGuidsAreNotSame(s1, s2);
            TestBase.CheckThatPtrsAreNotSame(s1, s2);
        }

        [Test]
        public void RecordingManager_GetRecordingFromCommand_FindsCorrectRecording()
        {
            var s1 = NewTestRecording(out Command c1, out Command c11);
            var s2 = NewTestRecording(out Command c2, out Command c22);

            var recording = RecordingManager.GetRecordingFromCommand(c22);

            Assert.AreEqual(s2, recording, "Recordings missmatched");
        }

        [Test]
        public void RecordingManager_GetRecordingFromCommandGuid_FindsCorrectRecording()
        {
            var s1 = NewTestRecording(out Command c1, out Command c11);
            var s2 = NewTestRecording(out Command c2, out Command c22);

            var recording = RecordingManager.GetRecordingFromCommandGuid(c22.Guid);

            Assert.AreEqual(s2, recording, "Recordings missmatched");
        }

        private void CheckIfRecordingsHasAllCorrectGuids(params Recording[] recordings)
        {
            var hashmapField = typeof(Recording).GetField("CommandGuidMap", BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var s in recordings)
            {
                var commands = s.Commands.GetAllNodes(false).Select(node => node.value);
                var hashmap = (HashSet<Guid>)hashmapField.GetValue(s);

                Assert.AreEqual(commands.Count(), hashmap.Count, $"Hashmap guid count missmatched with command count: {s.Name}");
                foreach (var c in commands)
                    Assert.IsTrue(s.HasRegisteredGuid(c.Guid), $"Command {c.Name} is not registered in recording {s.Name}");
            }
        }
    }
}
