using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotRuntime;
using RobotRuntime.Utils;
using System;

namespace Tests.Utils
{
    [TestClass]
    public class PathTests
    {
        /*[TestMethod]
        public void TestDifferencesDueToBackstepsDontMatter()
        {
            var format1 = Paths.NormalizeFilepath("c:\\windows\\system32");
            var format2 = Paths.NormalizeFilepath("c:\\Program Files\\..\\Windows\\System32");

            Assert.AreEqual(format1, format2);
        }*/

        [TestMethod]
        public void TestDifferencesInCapitolizationDontMatter()
        {
            var format1 = Paths.NormalizePath("c:\\windows\\system32\r");
            var format2 = Paths.NormalizePath("c:\\windows\\system32 ");

            Assert.AreEqual(format1, format2);
        }

        [TestMethod]
        public void TestDifferencesInFinalSlashDontMatter()
        {
            var format1 = Paths.NormalizePath("c:\\windows\\system32");
            var format2 = Paths.NormalizePath("c:\\windows\\system32\\");

            Assert.AreEqual(format1, format2);
        }

        [TestMethod]
        public void TestCanCalculateRelativePath()
        {
            var rootPath = "c:\\windows";
            var fullPath = "c:\\windows\\system32\\wininet.dll";
            var expectedResult = "system32\\wininet.dll";

            Environment.CurrentDirectory = rootPath;
            var result = Paths.GetRelativePath(fullPath);

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void TestThrowsExceptionIfRootDoesntMatchFullPath()
        {
            Logger.Instance = new FakeLogger();
            var rootPath = "c:\\windows";
            var fullPath = "c:\\program files\\Internet Explorer\\iexplore.exe";

            Environment.CurrentDirectory = rootPath;
            var result = Paths.GetRelativePath(fullPath);

            Assert.IsTrue(string.IsNullOrEmpty(result));
        }
    }
}
