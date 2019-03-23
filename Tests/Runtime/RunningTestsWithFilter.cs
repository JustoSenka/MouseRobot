using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Logging;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using RobotRuntime.Utils;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Unity;

namespace Tests.Runtime
{
    [TestFixture]
    public class RunningTestsWithFilter
    {
        private string TempProjectPath;

        ITestFixtureManager TestFixtureManager;
        IAssetManager AssetManager;
        ITestRunner TestRunner;
        IScriptManager ScriptManager;
        ICommandFactory CommandFactory;

        ILogger Logger;

        private const string k_CustomCommandPath = "Assets\\CommandLog.cs";
        private const string k_TestFixturePath = "Assets\\fixture.mrt";
        private string ExecutablePath => Path.Combine(Paths.ApplicationInstallPath, "RobotRuntime.exe");

        [SetUp]
        public void Initialize()
        {
            TempProjectPath = TestBase.GenerateProjectPath();
            var container = TestBase.ConstructContainerForTests();

            var ProjectManager = container.Resolve<IProjectManager>();

            TestFixtureManager = container.Resolve<ITestFixtureManager>();
            AssetManager = container.Resolve<IAssetManager>();
            TestRunner = container.Resolve<ITestRunner>();
            ScriptManager = container.Resolve<IScriptManager>();
            CommandFactory = container.Resolve<ICommandFactory>();

            Logger = container.Resolve<ILogger>();

            ProjectManager.InitProject(TempProjectPath);
        }

        [Test]
        public void TestRunner_RunningCustomRecording_Works()
        {
            File.WriteAllText(k_CustomCommandPath, Properties.Resources.CommandLog);
            AssetManager.Refresh();
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            CreateTestFixture();

            TestRunner.LoadSettings();
            TestRunner.StartTests("Test1").Wait();
            var logs = Logger.LogList.Where(log => log.Header.Contains("CommandLog")).ToArray();

            AssertIfCommandLogsAreOutOfOrder(logs, 1, 2, 11, 3, 4);
        }

        [Test]
        public void CommandLine_RunningCustomRecording_Works()
        {
            File.WriteAllText(k_CustomCommandPath, Properties.Resources.CommandLog);
            AssetManager.Refresh();
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            CreateTestFixture();

            var res = ProcessUtility.StartFromCommandLine(ExecutablePath, $"-p {TempProjectPath} -f Test1");
            var logs = FakeLogger.CreateLogsFromConsoleOutput(res).Where(log => log.Header.Contains("CommandLog")).ToArray();

            AssertIfCommandLogsAreOutOfOrder(logs, 1, 2, 11, 3, 4);
        }

        private void CreateTestFixture()
        {
            var f = new LightTestFixture
            {
                Name = "Fixture",
                Setup = CreateTestRecording(2),
                TearDown = CreateTestRecording(3),
                OneTimeSetup = CreateTestRecording(1),
                OneTimeTeardown = CreateTestRecording(4),
            };

            f.Setup.Name = LightTestFixture.k_Setup;
            f.TearDown.Name = LightTestFixture.k_TearDown;
            f.OneTimeSetup.Name = LightTestFixture.k_OneTimeSetup;
            f.OneTimeTeardown.Name = LightTestFixture.k_OneTimeTeardown;
            f.Tests = new Recording[] { CreateTestRecording(11, "Test1"), CreateTestRecording(22, "Test2") }.ToList();

            var t = TestFixtureManager.NewTestFixture(f);
            TestFixtureManager.SaveTestFixture(t, k_TestFixturePath);
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
