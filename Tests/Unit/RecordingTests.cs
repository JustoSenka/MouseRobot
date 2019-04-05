using NUnit.Framework;
using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Commands;
using RobotRuntime.Recordings;
using System.Linq;
using Unity;

namespace Tests.Unit
{
    [TestFixture]
    public class RecordingTests : TestWithCleanup
    {
        IHierarchyManager RecordingManager;

        [SetUp]
        public void Initialize()
        {
            var container = TestUtils.ConstructContainerForTests();
            var mr = container.Resolve<IMouseRobot>();
            RecordingManager = container.Resolve<IHierarchyManager>();
        }

        [Test]
        public void NewlyCreatedRecordingManager_WillHaveOneRecordingOpen()
        {
            Assert.AreEqual(1, RecordingManager.LoadedRecordings.Count);
        }

        [Test]
        public void Recording_MoveCommandAfter_WorksWithinSameLevel()
        {
            var s = RecordingManager.LoadedRecordings[0];

            var c1 = s.AddCommand(new CommandSleep(1));
            var c2 = s.AddCommand(new CommandSleep(2));
            var c3 = s.AddCommand(new CommandSleep(3));

            s.MoveCommandAfter(c1, c3);

            Assert.AreEqual(c2, s.Commands.GetChild(0).value);
            Assert.AreEqual(c3, s.Commands.GetChild(1).value);
            Assert.AreEqual(c1, s.Commands.GetChild(2).value);
            TestUtils.CheckThatGuidMapIsCorrect(s);
        }


        [Test]
        public void Recording_MoveCommandBefore_WorksWithinSameLevel()
        {
            var s = RecordingManager.LoadedRecordings[0];

            var c1 = s.AddCommand(new CommandSleep(1));
            var c2 = s.AddCommand(new CommandSleep(2));
            var c3 = s.AddCommand(new CommandSleep(3));

            s.MoveCommandBefore(c1, c3);

            Assert.AreEqual(c2, s.Commands.GetChild(0).value);
            Assert.AreEqual(c1, s.Commands.GetChild(1).value);
            Assert.AreEqual(c3, s.Commands.GetChild(2).value);
            TestUtils.CheckThatGuidMapIsCorrect(s);
        }

        [Test]
        public void Recording_MoveCommand_MultipleTimes_WorksWithinSameLevel()
        {
            var s = RecordingManager.LoadedRecordings[0];

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
            TestUtils.CheckThatGuidMapIsCorrect(s);
        }

        [Test]
        public void Recording_MoveCommandAfter_WorksToUpperLevel()
        {
            var s = RecordingManager.LoadedRecordings[0];

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
            TestUtils.CheckThatGuidMapIsCorrect(s);
        }

        [Test]
        public void Recording_MoveCommandBefore_WorksToUpperLevel()
        {
            var s = RecordingManager.LoadedRecordings[0];

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
            TestUtils.CheckThatGuidMapIsCorrect(s);
        }

        [Test]
        public void Recording_MoveCommandAfter_WorksToLowerLevel()
        {
            var s = RecordingManager.LoadedRecordings[0];

            var c1 = s.AddCommand(new CommandSleep(1));

            var n1 = s.Commands.GetNodeFromValue(c1);
            var c11 = s.AddCommand(new CommandSleep(2), c1);
            var c12 = s.AddCommand(new CommandSleep(3), c1);

            s.MoveCommandAfter(c12, c1);

            Assert.AreEqual(2, s.Commands.Count());
            Assert.AreEqual(1, n1.Count());
            Assert.AreEqual(c1, s.Commands.GetChild(0).value);
            Assert.AreEqual(c12, s.Commands.GetChild(1).value);
            TestUtils.CheckThatGuidMapIsCorrect(s);
        }

        [Test]
        public void Recording_MoveCommandBefore_WorksToLowerLevel()
        {
            var s = RecordingManager.LoadedRecordings[0];

            var c1 = s.AddCommand(new CommandSleep(1));

            var n1 = s.Commands.GetNodeFromValue(c1);
            var c11 = s.AddCommand(new CommandSleep(2), c1);
            var c12 = s.AddCommand(new CommandSleep(3), c1);

            s.MoveCommandBefore(c12, c1);

            Assert.AreEqual(2, s.Commands.Count());
            Assert.AreEqual(1, n1.Count());
            Assert.AreEqual(c12, s.Commands.GetChild(0).value);
            Assert.AreEqual(c1, s.Commands.GetChild(1).value);
            TestUtils.CheckThatGuidMapIsCorrect(s);
        }

        [Test]
        public void Recording_MoveNestedCommands_MovesAllChildCommandsAlso()
        {
            var s = RecordingManager.LoadedRecordings[0];

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
            TestUtils.CheckThatGuidMapIsCorrect(s);
        }

        [Test]
        public void Recording_InsertCommand_InsertsCommandInCorrectPosition()
        {
            var s = RecordingManager.LoadedRecordings[0];

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
            TestUtils.CheckThatGuidMapIsCorrect(s);
        }

        [Test]
        public void Recording_AddCommandNode_AddAllCommandsWithIt()
        {
            var s = RecordingManager.LoadedRecordings[0];

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
            TestUtils.CheckThatGuidMapIsCorrect(s);
        }

        [Test]
        public void Recording_RemoveCommand_RemovesItWithChildren()
        {
            var s = RecordingManager.LoadedRecordings[0];

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
            TestUtils.CheckThatGuidMapIsCorrect(s);
        }

        [Test]
        public void Recording_InsertCommandNode_AfterTopLevelCommand()
        {
            var s = RecordingManager.LoadedRecordings[0];
            var c0 = s.AddCommand(new CommandSleep(1));
            var c1 = s.AddCommand(new CommandSleep(1));
            var c12 = s.AddCommand(new CommandSleep(2), c1);

            var node = s.Commands.GetNodeFromValue(c1);

            s.RemoveCommand(c1);
            s.InsertCommandNodeAfter(node, c0);

            Assert.AreEqual(2, s.Commands.Count());
            Assert.AreEqual(c0, s.Commands.GetChild(0).value);
            Assert.AreEqual(c1, s.Commands.GetChild(1).value);
            Assert.AreEqual(c12, s.Commands.GetChild(1).GetChild(0).value);
            TestUtils.CheckThatGuidMapIsCorrect(s);
        }

        [Test]
        public void Recording_InsertCommandNode_BeforeTopLevelCommand()
        {
            var s = RecordingManager.LoadedRecordings[0];
            var c0 = s.AddCommand(new CommandSleep(1));
            var c1 = s.AddCommand(new CommandSleep(1));
            var c12 = s.AddCommand(new CommandSleep(2), c1);

            var node = s.Commands.GetNodeFromValue(c1);

            s.RemoveCommand(c1);
            s.InsertCommandNodeBefore(node, c0);

            Assert.AreEqual(2, s.Commands.Count());
            Assert.AreEqual(c0, s.Commands.GetChild(1).value);
            Assert.AreEqual(c1, s.Commands.GetChild(0).value);
            Assert.AreEqual(c12, s.Commands.GetChild(0).GetChild(0).value);
            TestUtils.CheckThatGuidMapIsCorrect(s);
        }

        [Test]
        public void Recording_InsertCommandNode_AfterBottomLevelCommand()
        {
            var s = RecordingManager.LoadedRecordings[0];
            var c1 = s.AddCommand(new CommandSleep(1));
            var c11 = s.AddCommand(new CommandSleep(2), c1);
            var c2 = s.AddCommand(new CommandSleep(3));
            var c21 = s.AddCommand(new CommandSleep(4), c2);

            var node = s.Commands.GetNodeFromValue(c2);

            s.RemoveCommand(c2);
            s.InsertCommandNodeAfter(node, c11);

            Assert.AreEqual(1, s.Commands.Count());
            var child = s.Commands.GetChild(0);

            Assert.AreEqual(2, child.Count());
            Assert.AreEqual(c1, child.value);
            Assert.AreEqual(c11, child.GetChild(0).value);
            Assert.AreEqual(c2, child.GetChild(1).value);
            Assert.AreEqual(c21, child.GetChild(1).GetChild(0).value);
            TestUtils.CheckThatGuidMapIsCorrect(s);
        }

        [Test]
        public void Recording_InsertCommandNode_BeforeBottomLevelCommand()
        {
            var s = RecordingManager.LoadedRecordings[0];
            var c1 = s.AddCommand(new CommandSleep(1));
            var c11 = s.AddCommand(new CommandSleep(2), c1);
            var c2 = s.AddCommand(new CommandSleep(3));
            var c21 = s.AddCommand(new CommandSleep(4), c2);

            var node = s.Commands.GetNodeFromValue(c2);

            s.RemoveCommand(c2);
            s.InsertCommandNodeBefore(node, c11);

            Assert.AreEqual(1, s.Commands.Count());
            var child = s.Commands.GetChild(0);

            Assert.AreEqual(2, child.Count());
            Assert.AreEqual(c1, child.value);
            Assert.AreEqual(c11, child.GetChild(1).value);
            Assert.AreEqual(c2, child.GetChild(0).value);
            Assert.AreEqual(c21, child.GetChild(0).GetChild(0).value);
            TestUtils.CheckThatGuidMapIsCorrect(s);
        }


        [Test]
        public void Guid_ToLightRecordingIsCorrect()
        {
            var recording = new Recording();
            var guid = recording.Guid;

            var lightRecording = recording.ToLightRecording();

            Assert.AreEqual(guid, lightRecording.Guid, "Guids should be the same");
            TestUtils.CheckThatGuidMapIsCorrect(recording);
        }

        [Test]
        public void Guid_FromLightRecordingIsCorrect()
        {
            var recording = new Recording();

            var lightRecording = recording.ToLightRecording();
            var newRecording = Recording.FromLightRecording(lightRecording);

            Assert.AreEqual(lightRecording.Guid, newRecording.Guid, "Guids should be the same");
            TestUtils.CheckThatGuidMapIsCorrect(recording);
        }

        [Test]
        public void Guid_FromLightRecordingCtorIsCorrect()
        {
            var recording = new Recording();

            var lightRecording = recording.ToLightRecording();
            var newRecording = new Recording(lightRecording);

            Assert.AreEqual(lightRecording.Guid, newRecording.Guid, "Guids should be the same");
            TestUtils.CheckThatGuidMapIsCorrect(recording);
        }

        [Test]
        public void Guids_AfterCloningRecording_AreSame()
        {
            var s1 = new Recording();
            var s2 = (Recording)s1.Clone();

            TestUtils.CheckThatGuidsAreSame(s1, s2);
            TestUtils.CheckThatPtrsAreNotSame(s1, s2);
            TestUtils.CheckThatGuidMapIsCorrect(s1);
            TestUtils.CheckThatGuidMapIsCorrect(s2);
        }
    }
}
