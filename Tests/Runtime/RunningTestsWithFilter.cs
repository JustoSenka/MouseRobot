using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using RobotRuntime.Abstractions;
using RobotRuntime.Logging;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using RobotRuntime.Utils;
using System.IO;
using System.Linq;
using Unity;

namespace Tests.Runtime
{
    [TestFixture]
    public class RunningTestsWithFilter : TestWithCleanup
    {
        private string TempProjectPath;

        ITestFixtureManager TestFixtureManager;
        IAssetManager AssetManager;
        ITestRunner TestRunner;
        IScriptManager ScriptManager;
        ICommandFactory CommandFactory;

        ILogger Logger;

        private const string k_CustomCommandPath = "Assets\\CommandLog.cs";
        private const string k_TestFixturePath1 = "Assets\\fixture1.mrt";
        private const string k_TestFixturePath2 = "Assets\\fixture2.mrt";
        private string ExecutablePath => Path.Combine(Paths.ApplicationInstallPath, "RobotRuntime.exe");

        [SetUp]
        public void Initialize()
        {
            TempProjectPath = TestUtils.GenerateProjectPath();
            var container = TestUtils.ConstructContainerForTests();

            var ProjectManager = container.Resolve<IProjectManager>();

            TestFixtureManager = container.Resolve<ITestFixtureManager>();
            AssetManager = container.Resolve<IAssetManager>();
            TestRunner = container.Resolve<ITestRunner>();
            ScriptManager = container.Resolve<IScriptManager>();
            CommandFactory = container.Resolve<ICommandFactory>();

            Logger = container.Resolve<ILogger>();

            ProjectManager.InitProject(TempProjectPath);
        }

        [TestCase("Test15", false, new[] { 11, 12, 15, 13, 14 })]
        [TestCase("Fixture1.Test15", false, new[] { 11, 12, 15, 13, 14 })]
        [TestCase("Test(15|16)", false, new[] { 11, 12, 15, 13, 12, 16, 13, 14 })]
        [TestCase("Fixture1", false, new[] { 11, 12, 15, 13, 12, 16, 13, 14 })]
        [TestCase("Fixture", false, new[] { 11, 12, 15, 13, 12, 16, 13, 14, 21, 22, 25, 23, 22, 26, 23, 24 })]
        [TestCase("", false, new[] { 11, 12, 15, 13, 12, 16, 13, 14, 21, 22, 25, 23, 22, 26, 23, 24 })]
        [TestCase("Test2", false, new[] { 21, 22, 25, 23, 22, 26, 23, 24 })]
        [TestCase("Fixture1|Test26", false, new[] { 11, 12, 15, 13, 12, 16, 13, 14, 21, 22, 26, 23, 24 })]

        [TestCase("Test15", true, new[] { 11, 12, 15, 13, 14 })]
        [TestCase("Fixture1.Test15", true, new[] { 11, 12, 15, 13, 14 })]
        [TestCase("Test(15|16)", true, new[] { 11, 12, 15, 13, 12, 16, 13, 14 })]
        [TestCase("Fixture1", true, new[] { 11, 12, 15, 13, 12, 16, 13, 14 })]
        [TestCase("Fixture", true, new[] { 11, 12, 15, 13, 12, 16, 13, 14, 21, 22, 25, 23, 22, 26, 23, 24 })]
        [TestCase("", true, new[] { 11, 12, 15, 13, 12, 16, 13, 14, 21, 22, 25, 23, 22, 26, 23, 24 })]
        [TestCase("Test2", true, new[] { 21, 22, 25, 23, 22, 26, 23, 24 })]
        [TestCase("Fixture1|Test26", true, new[] { 11, 12, 15, 13, 12, 16, 13, 14, 21, 22, 26, 23, 24 })]
        public void RunningTests_WithFilter(string filter, bool useCommandLine, int[] expectedOrder)
        {
            AssetManager.CreateAsset(Properties.Resources.CommandLog, k_CustomCommandPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            PrepareProjectWithTestFixtures();
            var logs = RunTestsAndGetLogs(filter, useCommandLine);

            AssertIfCommandLogsAreOutOfOrder(logs, expectedOrder);
        }

        private Log[] RunTestsAndGetLogs(string filter, bool useCommandLine)
        {
            if (useCommandLine)
            {
                var args = filter.IsEmpty() ? $"-p {TempProjectPath}" : $"-p {TempProjectPath} -f {filter}";
                var res = ProcessUtility.StartFromCommandLine(ExecutablePath, args);
                return FakeLogger.CreateLogsFromConsoleOutput(res).Where(log => log.Header.Contains("CommandLog")).ToArray();
            }
            else
            {
                // TestRunner.LoadSettings();
                TestRunner.StartTests(filter).Wait();
                return Logger.LogList.Where(log => log.Header.Contains("CommandLog")).ToArray();
            }
        }

        private void PrepareProjectWithTestFixtures()
        {
            var f1 = CreateLightTestFixture("Fixture1", 10);
            var f2 = CreateLightTestFixture("Fixture2", 20);

            TestFixtureManager.SaveTestFixture(TestFixtureManager.NewTestFixture(f1), k_TestFixturePath1);
            TestFixtureManager.SaveTestFixture(TestFixtureManager.NewTestFixture(f2), k_TestFixturePath2);
        }

        private LightTestFixture CreateLightTestFixture(string name, int fixIndex)
        {
            var f = new LightTestFixture
            {
                Name = name,
                OneTimeSetup = CreateTestRecording(fixIndex + 1),
                Setup = CreateTestRecording(fixIndex + 2),
                TearDown = CreateTestRecording(fixIndex + 3),
                OneTimeTeardown = CreateTestRecording(fixIndex + 4),
            };

            f.Setup.Name = LightTestFixture.k_Setup;
            f.TearDown.Name = LightTestFixture.k_TearDown;
            f.OneTimeSetup.Name = LightTestFixture.k_OneTimeSetup;
            f.OneTimeTeardown.Name = LightTestFixture.k_OneTimeTeardown;
            f.Tests = new Recording[] { CreateTestRecording(fixIndex + 5, $"Test{fixIndex + 5}"), CreateTestRecording(fixIndex + 6, $"Test{fixIndex + 6}") }.ToList();
            return f;
        }

        private Recording CreateTestRecording(int number, string name = "")
        {
            var r = new Recording();
            var command = CommandFactory.Create("CommandLog");
            command.SetFieldIfExist("Number", number);
            r.AddCommand(command);
            r.Name = name;
            return r;
        }

        private void AssertIfCommandLogsAreOutOfOrder(Log[] logs, params int[] logNr)
        {
            Assert.AreEqual(logNr.Length, logs.Length, "Log count missmatched");
            foreach (var (log, nr) in logs.Zip(logNr, (l, n) => (l, n)))
                Assert.AreEqual($"CommandLog: {nr}", log.Header);
        }
    }
}
