using NUnit.Framework;
using RobotRuntime.Abstractions;
using RobotRuntime.Logging;
using System.Drawing;

namespace Tests.Unit
{
    [TestFixture]
    public class StatusManagerTests : TestWithCleanup
    {
        IStatusManager StatusManager;

        Status defaultStatus;
        Status statusA = new Status("a", "a", Color.Blue);
        Status statusB = new Status("b", "b", Color.Blue);
        Status statusC = new Status("c", "c", Color.Blue);

        [SetUp]
        public void CreateStatusManager()
        {
            StatusManager = new StatusManager();
            defaultStatus = StatusManager.Status;
            statusFromCallback = default(Status);
        }

        [Test]
        public void StatusManager_DefaultStatus_IsNotNullStruct()
        {
            Assert.AreNotEqual(default(Status), StatusManager.Status, "Default status should always be there");
        }

        [Test]
        public void StatusManager_DefaultStatus_IsPriorityOf_10()
        {
            StatusManager.Add("a", 11, statusA);
            Assert.AreEqual(defaultStatus, StatusManager.Status, "Default status should have been returned");
        }

        [Test]
        public void StatusManager_Status_GivesTopPrioritizedStatus()
        {
            StatusManager.Add("a", 8, statusA);
            StatusManager.Add("b", 5, statusB);
            StatusManager.Add("c", 7, statusC);

            Assert.AreEqual(statusB, StatusManager.Status, "Status structs missmatched");
        }

        [Test]
        public void StatusManager_Status_GivesLastStatusAdded_IfPrioritiesAreEqual()
        {
            StatusManager.Add("a", 8, statusA);
            StatusManager.Add("b", 5, statusB);
            StatusManager.Add("c", 5, statusC);

            Assert.AreEqual(statusC, StatusManager.Status, "Status structs missmatched");
        }

        [Test]
        public void StatusManager_OverridingStatus_WithSameName_DoNotAddAditionalElements()
        {
            StatusManager.Add("a", 5, statusA);
            StatusManager.Add("a", 5, statusB);

            Assert.AreEqual(statusB, StatusManager.Status, "Status structs missmatched");

            StatusManager.Remove("a");
            Assert.AreEqual(defaultStatus, StatusManager.Status, "Default status should have been returned");
        }

        [Test]
        public void StatusManager_OverridingStatus_WithSameName_AndDifferentPriorities_DoNotAddAditionalElements()
        {
            StatusManager.Add("a", 5, statusA);
            StatusManager.Add("a", 8, statusB);

            Assert.AreEqual(statusB, StatusManager.Status, "Status structs missmatched");

            StatusManager.Remove("a");
            Assert.AreEqual(defaultStatus, StatusManager.Status, "Default status should have been returned");
        }

        [Test]
        public void StatusManager_RemovingStatuses_WillBringBackLessPrioritizeStatuses()
        {
            StatusManager.Add("a", 5, statusA);
            StatusManager.Add("b", 2, statusB);
            StatusManager.Add("c", 8, statusC);

            Assert.AreEqual(statusB, StatusManager.Status, "Status structs missmatched");
            StatusManager.Remove("b");

            Assert.AreEqual(statusA, StatusManager.Status, "Status structs missmatched");
            StatusManager.Remove("a");

            Assert.AreEqual(statusC, StatusManager.Status, "Status structs missmatched");
            StatusManager.Remove("c");

            Assert.AreEqual(defaultStatus, StatusManager.Status, "Default status should have been returned");
        }

        Status statusFromCallback;

        [Test]
        public void StatusManager_StatusUpdatedEvent_WillFireWithCorrectStatus()
        {
            StatusManager.Add("a", 5, statusA);
            StatusManager.Add("b", 8, statusB);

            StatusManager.StatusUpdated += EventCallback;

            StatusManager.Add("c", 2, statusC);
            Assert.AreEqual(statusC, statusFromCallback, "Status structs missmatched");

            StatusManager.StatusUpdated -= EventCallback;
        }

        [Test]
        public void StatusManager_StatusUpdatedEvent_WillFireWithCorrectStatus_IfLowerPriorityIsAdded()
        {
            StatusManager.Add("a", 2, statusA);
            StatusManager.Add("b", 6, statusB);

            StatusManager.StatusUpdated += EventCallback;

            StatusManager.Add("c", 4, statusC);
            Assert.AreEqual(statusA, statusFromCallback, "Status structs missmatched");

            StatusManager.StatusUpdated -= EventCallback;
        }

        [Test]
        public void StatusManager_StatusUpdatedEvent_ForOverridingStatus_IsCalledWithCorrectStatus()
        {
            StatusManager.Add("a", 4, statusA);
            StatusManager.Add("b", 2, statusB);

            StatusManager.StatusUpdated += EventCallback;

            StatusManager.Add("b", 6, statusC);
            Assert.AreEqual(statusA, statusFromCallback, "Status structs missmatched");

            StatusManager.StatusUpdated -= EventCallback;
        }

        [Test]
        public void StatusManager_StatusUpdatedEvent_ForOverridingStatus_IsCalledWithCorrectStatus_WhenRaisingPriority()
        {
            StatusManager.Add("a", 4, statusA);
            StatusManager.Add("b", 6, statusB);

            StatusManager.StatusUpdated += EventCallback;
            StatusManager.Add("b", 2, statusC);
            Assert.AreEqual(statusC, statusFromCallback, "Status structs missmatched");

            StatusManager.StatusUpdated -= EventCallback;
        }

        [Test]
        public void StatusManager_StatusUpdatedEvent_ForRemovingStatus_IsCalledWithCorrectValue()
        {
            StatusManager.Add("a", 4, statusA);
            StatusManager.Add("b", 6, statusB);

            StatusManager.StatusUpdated += EventCallback;
            StatusManager.Remove("b");
            Assert.AreEqual(statusA, statusFromCallback, "Status structs missmatched");

            StatusManager.StatusUpdated -= EventCallback;
        }

        [Test]
        public void StatusManager_StatusUpdatedEvent_ForRemovingStatus_WillReturnDefaultIfItsPriorityIsTheHighest()
        {
            StatusManager.Add("a", 15, statusA);
            StatusManager.Add("b", 20, statusB);

            StatusManager.StatusUpdated += EventCallback;
            StatusManager.Remove("b");
            Assert.AreEqual(defaultStatus, statusFromCallback, "Status structs missmatched");

            StatusManager.StatusUpdated -= EventCallback;
        }

        private void EventCallback(Status obj)
        {
            statusFromCallback = obj;
        }
    }
}
