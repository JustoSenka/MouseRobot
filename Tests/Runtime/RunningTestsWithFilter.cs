using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using RobotRuntime.Abstractions;
using RobotRuntime.Tests;
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
        ITestStatusManager TestStatusManager;

        ILogger Logger;

        private const string k_CustomCommandPath = "Assets\\CommandLog.cs";

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
            TestStatusManager = container.Resolve<ITestStatusManager>();

            Logger = container.Resolve<ILogger>();

            ProjectManager.InitProject(TempProjectPath).Wait();
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

            TestFixtureUtils.PrepareProjectWithTestFixtures(TestFixtureManager, CommandFactory);
            var logs = TestFixtureUtils.RunTestsAndGetLogs(TestRunner, filter, useCommandLine, TempProjectPath, TestStatusManager.OutputFilePath);

            TestFixtureUtils.AssertIfCommandLogsAreOutOfOrder(logs, expectedOrder);
        }
    }
}
