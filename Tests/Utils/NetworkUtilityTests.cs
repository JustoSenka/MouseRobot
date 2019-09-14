using NUnit.Framework;
using Robot.Analytics;
using Robot.Analytics.Abstractions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity;

namespace Tests.Utils
{
    [TestFixture]
    public class NetworkUtilityTests : TestWithCleanup
    {
        private Network Network;
        private INetwork NetworkInterface;

        [SetUp]
        public void Initialize()
        {
            var container = TestUtils.ConstructContainerForTests();
            NetworkInterface = container.Resolve<INetwork>();
            Network = container.Resolve<Network>();
        }

        // Some tests are disabled because Rest service provides only limited amount of requests per day

        public static IEnumerable<TestCaseData> AllIPSources()
        {
            var Network = new Network();
            yield return new TestCaseData(Network.GetExternalIP_1()).SetName("Network_GetIP_FollowsCorrectFormat(External IP 1)");
            yield return new TestCaseData(Network.GetExternalIP_2()).SetName("Network_GetIP_FollowsCorrectFormat(External IP 2)");
            yield return new TestCaseData(Network.GetExternalIP_3()).SetName("Network_GetIP_FollowsCorrectFormat(External IP 3)");
        }

        public static IEnumerable<TestCaseData> AllCountryCodes()
        {
            var Network = new Network();
            var ip = Network.GetExternalIP();
            yield return new TestCaseData(Network.GetCountryID_1(ip)).SetName("Network_GetCountry_FollowsCorrectFormat(Country 1)");
            yield return new TestCaseData(Network.GetCountryID_2(ip)).SetName("Network_GetCountry_FollowsCorrectFormat(Country 2)");
            yield return new TestCaseData(Network.GetCountryID_3(ip)).SetName("Network_GetCountry_FollowsCorrectFormat(Country 3)");
        }

        //[TestCaseSource("AllIPSources")]
        public void Network_GetIP_FollowsCorrectFormat(string ip)
        {
            Console.WriteLine(ip);
            Assert.IsTrue(Regex.IsMatch(ip, Network.k_IpRegex), "IP did not match regex for IP format: " + ip);
        }

        //[TestCaseSource("AllCountryCodes")]
        public void Network_GetCountry_FollowsCorrectFormat(string id)
        {
            Console.WriteLine(id);
            Assert.IsTrue(Regex.IsMatch(id, Network.k_CountryIDRegex), "ID did not match regex for CountryID format: " + id);
        }

        [Test]
        public void AtLeastOneIP_FromAllDifferentSources_IsCorrect()
        {
            var ip = Network.GetExternalIP();

            Console.WriteLine($"IP 1: {ip}");
            Assert.IsTrue(Regex.IsMatch(ip, Network.k_IpRegex), "IP did not match regex for IP format: " + ip);
        }

        [Test]
        public void AtLeastOneIP_FromAllDifferentSources_IsCorrect_Interface()
        {
            var ip = NetworkInterface.GetExternalIP();

            Console.WriteLine($"IP 1: {ip}");
            Assert.IsTrue(Regex.IsMatch(ip, Network.k_IpRegex), "IP did not match regex for IP format: " + ip);
        }

        // [Test]
        public void GettingExternalIP_FromDifferentSources_AreAllIdentical()
        {
            var ip1 = Network.GetExternalIP_1();
            var ip2 = Network.GetExternalIP_2();
            var ip3 = Network.GetExternalIP_3();

            Console.WriteLine($"IP 1: {ip1}");
            Console.WriteLine($"IP 2: {ip2}");
            Console.WriteLine($"IP 3: {ip3}");

            Assert.AreEqual(ip1, ip2, "IP1 != IP2");
            Assert.AreEqual(ip2, ip3, "IP2 != IP3");
        }

        [Test]
        public void AtLeastOneCountryID_FromAllDifferentSources_IsCorrect_Interface()
        {
            var id = NetworkInterface.GetCountryID();

            Console.WriteLine($"ID 1: {id}");
            Assert.IsTrue(Regex.IsMatch(id, Network.k_CountryIDRegex), "ID did not match regex for ID format: " + id);
        }

        // [Test]
        public void GettingCountryID_FromDifferentSources_AreAllIdentical()
        {
            var ip = Network.GetExternalIP();
            var id1 = Network.GetCountryID_1(ip);
            var id2 = Network.GetCountryID_2(ip);
            var id3 = Network.GetCountryID_3(ip);

            Console.WriteLine($"ID 1: {id1}");
            Console.WriteLine($"ID 2: {id2}");
            Console.WriteLine($"ID 3: {id3}");

            Assert.AreEqual(id1, id2, "ID1 != ID2");
            Assert.AreEqual(id2, id3, "ID2 != ID3");
        }
    }
}
