using NUnit.Framework;
using NUnit.Framework.Internal;
using Robot.Abstractions;
using Unity;
using RobotRuntime;
using Logger = RobotRuntime.Logger;
using System;
using System.Security.Cryptography;
using System.IO;

namespace Tests.Unit
{
    public class AnalyticsTests : TestWithCleanup
    {
        private IAnalytics Analytics;
        private IReceiveTrackingID IDReceiver;
        private IUserIdentity UserIdentity;



        [SetUp]
        public void Initialize()
        {
            var container = TestUtils.ConstructContainerForTests();
            var mr = container.Resolve<IMouseRobot>();

            Analytics = container.Resolve<IAnalytics>();
            IDReceiver = container.Resolve<IReceiveTrackingID>();
            UserIdentity = container.Resolve<IUserIdentity>();
        }

        [Test]
        public void MachineID_IsNotEmpty()
        {
            var id = UserIdentity.GetMachineID();
            Logger.Log(LogType.Log, id);
            Assert.AreNotEqual(string.Empty, id);
            Assert.AreNotEqual("0", id);
        }

        [Test]
        public void MachineID_IsDeterministic()
        {
            var id1 = UserIdentity.GetMachineID();
            var id2 = UserIdentity.GetMachineID();
            Assert.AreEqual(id1, id2);
        }
    }
}
