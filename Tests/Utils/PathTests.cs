using NUnit.Framework;
using RobotRuntime;
using RobotRuntime.Utils;
using System;

namespace Tests.Utils
{
    [TestFixture]
    public class PathTests
    {
        /*[Test]
        public void TestDifferencesDueToBackstepsDontMatter()
        {
            var format1 = Paths.NormalizeFilepath("c:\\windows\\system32");
            var format2 = Paths.NormalizeFilepath("c:\\Program Files\\..\\Windows\\System32");

            Assert.AreEqual(format1, format2);
        }*/

        [Test]
        public void TestDifferencesInCapitolizationDontMatter()
        {
            var format1 = Paths.NormalizePath("c:\\windows\\system32\r");
            var format2 = Paths.NormalizePath("c:\\windows\\system32 ");

            Assert.AreEqual(format1, format2);
        }

        [Test]
        public void TestDifferencesInFinalSlashDontMatter()
        {
            var format1 = Paths.NormalizePath("c:\\windows\\system32");
            var format2 = Paths.NormalizePath("c:\\windows\\system32\\");

            Assert.AreEqual(format1, format2);
        }

        [Test]
        public void TestCanCalculateRelativePath()
        {
            var rootPath = "c:\\windows";
            var fullPath = "c:\\windows\\system32\\wininet.dll";
            var expectedResult = "system32\\wininet.dll";

            Environment.CurrentDirectory = rootPath;
            var result = Paths.GetRelativePath(fullPath);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestThrowsExceptionIfRootDoesntMatchFullPath()
        {
            Logger.Instance = new FakeLogger();
            var rootPath = "c:\\windows";
            var fullPath = "c:\\program files\\Internet Explorer\\iexplore.exe";

            Environment.CurrentDirectory = rootPath;
            var result = Paths.GetRelativePath(fullPath);

            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [Test]
        public void GetPathDirectoryElementsWtihFileName_ReturnCorrectElements()
        {
            var path = "Assets/scripts/sc.mrb";
            var expected = new[] { "Assets", "scripts", "sc.mrb" };

            var res = Paths.GetPathDirectoryElementsWtihFileName(path);

            CollectionAssert.AreEqual(expected, res);
        }

        [Test]
        public void GetPathDirectoryElements_ReturnCorrectElements()
        {
            var path = "Assets/scripts/sc.mrb";
            var expected = new[] { "Assets", "scripts" };

            var res = Paths.GetPathDirectoryElements(path);

            CollectionAssert.AreEqual(expected, res);
        }

        [Test]
        public void JoinDirectoryElementsIntoPaths_ReturnCorrectElements()
        {
            var path = "Assets/scripts/sc.mrb".NormalizePath();
            var expected = new[] { "Assets", "Assets/scripts".NormalizePath(), "Assets/scripts/sc.mrb".NormalizePath() };

            var elements = Paths.GetPathDirectoryElementsWtihFileName(path);
            var res = Paths.JoinDirectoryElementsIntoPaths(elements);

            CollectionAssert.AreEqual(expected, res);
        }

        [Test]
        public void GetParentPath_ReturnsCorrectPath_OnFile()
        {
            var path = "Assets/scripts/sc.mrb".NormalizePath();
            var parentPath = "Assets/scripts".NormalizePath();

            var res = Paths.GetPathParent(path);

            Assert.AreEqual(parentPath, res);
        }

        [Test]
        public void GetParentPath_ReturnsCorrectPath_OnFolder()
        {
            var path = "Assets/scripts/some folder".NormalizePath();
            var parentPath = "Assets/scripts".NormalizePath();

            var res = Paths.GetPathParent(path);

            Assert.AreEqual(parentPath, res);
        }
    }
}
