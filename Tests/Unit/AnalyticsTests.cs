using NUnit.Framework;
using NUnit.Framework.Internal;
using Robot.Abstractions;
using Robot.Analytics.Abstractions;
using RobotRuntime;
using System.Text.RegularExpressions;
using Unity;
using Logger = RobotRuntime.Logger;

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

            Analytics = container.Resolve<IAnalytics>();
            IDReceiver = container.Resolve<IReceiveTrackingID>();
            UserIdentity = container.Resolve<IUserIdentity>();
        }

        [Test]
        public void MachineID_IsNotEmpty_AndDeterministic()
        {
            var id = UserIdentity.GetMachineID();
            var id2 = UserIdentity.GetMachineID();
            AssertUserIdentity(id, id2, @".*");
        }

        [Test]
        public void CpuID_IsNotEmpty_AndDeterministic()
        {
            var id = UserIdentity.GetCpuID();
            var id2 = UserIdentity.GetCpuID();
            AssertUserIdentity(id, id2, @"Model|Intel|AMD|Family");
        }

        [Test]
        public void MacAddress_IsNotEmpty_AndDeterministic()
        {
            var id = UserIdentity.GetMacAddress();
            var id2 = UserIdentity.GetMacAddress();
            AssertUserIdentity(id, id2, @".*");
        }

        [Test]
        public void OS_IsNotEmpty_AndDeterministic()
        {
            var id = UserIdentity.GetOperatingSystem();
            var id2 = UserIdentity.GetOperatingSystem();
            AssertUserIdentity(id, id2, @"Windows|OSX|Linux");
        }

        [Test]
        public void Resolution_IsNotEmpty_AndDeterministic()
        {
            var id = UserIdentity.GetScreenResolution();
            var id2 = UserIdentity.GetScreenResolution();
            AssertUserIdentity(id, id2, @"\d{2,5}x\d{2,5}");
        }

        [Test]
        public void CountryID_IsNotEmpty_AndDeterministic()
        {
            var id = UserIdentity.GetCountryID();
            var id2 = UserIdentity.GetCountryID();
            AssertUserIdentity(id, id2, @"\w{2}");
        }

        [Test]
        public void PushAnalyticsEvent_IsSuccessfull()
        {
            var res = Analytics.PushEvent("TestCategory", "TestAction", "TestLabel", 5).Result;
            Assert.IsTrue(res, "Http post request failed");
        }

        [Test]
        public void AnalyticsID_IsInCorrectFormat()
        {
            var id = IDReceiver.ID;
            Assert.IsFalse(id.IsEmpty());
            Assert.IsFalse("0" == id);
            Assert.IsTrue(Regex.IsMatch(id, @"UA\-\d{9}\-\d{1,3}", RegexOptions.IgnoreCase));
        }

        private static void AssertUserIdentity(string id, string id2, string regexFilter)
        {
            Logger.Log(LogType.Log, id);
            Assert.AreNotEqual(string.Empty, id);
            Assert.AreNotEqual("0", id);

            Assert.IsTrue(Regex.IsMatch(id, regexFilter, RegexOptions.IgnoreCase));
            Assert.AreEqual(id, id2, "Not deterministic");
        }
    }
}
