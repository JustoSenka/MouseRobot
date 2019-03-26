using NUnit.Framework;
using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Logging;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using RobotRuntime.Utils;
using System;
using System.IO;
using System.Linq;

namespace Tests
{
    public static class TestFixtureUtils
    {
        public static string ExecutablePath => Path.Combine(Paths.ApplicationInstallPath, "RobotRuntime.exe");
        public const string k_TestFixturePath1 = "Assets\\fixture1.mrt";
        public const string k_TestFixturePath2 = "Assets\\fixture2.mrt";

        public static Log[] RunRecordingAndGetLogs(ITestRunner TestRunner, string recordingPath, bool useCommandLine, string projectPath, string outputFilePath)
        {
            if (useCommandLine)
            {
                var args = $"-p {projectPath} -r {recordingPath} -o {outputFilePath}";
                var res = ProcessUtility.StartFromCommandLine(ExecutablePath, args);
                return FakeLogger.CreateLogsFromConsoleOutput(res).Where(log => log.Header.Contains("CommandLog")).ToArray();
            }
            else
            {
                TestRunner.StartRecording(recordingPath).Wait();
                return Logger.Instance.LogList.Where(log => log.Header.Contains("CommandLog")).ToArray();
            }
        }

        public static Log[] RunTestsAndGetLogs(ITestRunner TestRunner, string filter, bool useCommandLine, string projectPath, string outputFilePath)
        {
            if (useCommandLine)
            {
                var args = filter.IsEmpty() ? $"-p {projectPath}" : $"-p {projectPath} -f {filter} -o {outputFilePath}";
                var res = ProcessUtility.StartFromCommandLine(ExecutablePath, args);
                return FakeLogger.CreateLogsFromConsoleOutput(res).Where(log => log.Header.Contains("CommandLog")).ToArray();
            }
            else
            {
                // TestRunner.LoadSettings();
                TestRunner.StartTests(filter).Wait();
                return Logger.Instance.LogList.Where(log => log.Header.Contains("CommandLog")).ToArray();
            }
        }

        public static void PrepareProjectWithTestFixtures(ITestFixtureManager TestFixtureManager, ICommandFactory CommandFactory)
        {
            var f1 = CreateLightTestFixture(CommandFactory, "Fixture1", 10);
            var f2 = CreateLightTestFixture(CommandFactory, "Fixture2", 20);

            TestFixtureManager.SaveTestFixture(TestFixtureManager.NewTestFixture(f1), k_TestFixturePath1);
            TestFixtureManager.SaveTestFixture(TestFixtureManager.NewTestFixture(f2), k_TestFixturePath2);
        }

        public static LightTestFixture CreateLightTestFixture(ICommandFactory CommandFactory, string name, int fixIndex = 0)
        {
            var f = new LightTestFixture
            {
                Name = name,
                OneTimeSetup = CreateTestRecording(CommandFactory, fixIndex + 1),
                Setup = CreateTestRecording(CommandFactory, fixIndex + 2),
                TearDown = CreateTestRecording(CommandFactory, fixIndex + 3),
                OneTimeTeardown = CreateTestRecording(CommandFactory, fixIndex + 4),
            };

            f.Setup.Name = LightTestFixture.k_Setup;
            f.TearDown.Name = LightTestFixture.k_TearDown;
            f.OneTimeSetup.Name = LightTestFixture.k_OneTimeSetup;
            f.OneTimeTeardown.Name = LightTestFixture.k_OneTimeTeardown;
            f.Tests = new Recording[] {
                CreateTestRecording(CommandFactory, fixIndex + 5, $"Test{fixIndex + 5}"),
                CreateTestRecording(CommandFactory, fixIndex + 6, $"Test{fixIndex + 6}") }.ToList();
            return f;
        }

        public static Recording CreateTestRecording(ICommandFactory CommandFactory, int number, string name = "")
        {
            var r = new Recording();
            var command = CreateCustomLogCommand(CommandFactory, number);
            r.AddCommand(command);
            r.Name = name;
            return r;
        }

        public static Command CreateCustomLogCommand(ICommandFactory CommandFactory, int number)
        {
            var command = CommandFactory.Create("CommandLog");
            command.SetFieldIfExist("Number", number);
            return command;
        }

        public static void AssertIfCommandLogsAreOutOfOrder(Log[] logs, params int[] logNr)
        {
            Assert.AreEqual(logNr.Length, logs.Length, "Log count missmatched");
            foreach (var (log, nr) in logs.Zip(logNr, (l, n) => (l, n)))
                Assert.AreEqual($"CommandLog: {nr}", log.Header);
        }
    }
}
