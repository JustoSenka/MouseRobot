using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Tests;
using System;
using System.IO;
using Unity;

namespace Tests.Runtime
{
    [TestFixture]
    public class RunningTestsProducesOutput : TestWithCleanup
    {
        private string TempProjectPath;

        ITestFixtureManager TestFixtureManager;
        IAssetManager AssetManager;
        ITestRunner TestRunner;
        IScriptManager ScriptManager;
        ICommandFactory CommandFactory;
        ITestStatusManager TestStatusManager;

        private const string k_CustomCommandPath = "Assets\\CommandLog.cs";

        [SetUp]
        public void Initialize()
        {
            TempProjectPath = TestUtils.GenerateProjectPath();
            var Container = TestUtils.ConstructContainerForTests();

            var ProjectManager = Container.Resolve<IProjectManager>();

            TestFixtureManager = Container.Resolve<ITestFixtureManager>();
            AssetManager = Container.Resolve<IAssetManager>();
            TestRunner = Container.Resolve<ITestRunner>();
            ScriptManager = Container.Resolve<IScriptManager>();
            CommandFactory = Container.Resolve<ICommandFactory>();
            TestStatusManager = Container.Resolve<ITestStatusManager>();

            Container.Resolve<ITestRunnerManager>(); // This one outputs test results to file, but is not referenced by anything

            ProjectManager.InitProjectNoScriptCompile(TempProjectPath);
        }

        private readonly static string[] m_Results = new[]
        {
            "Passed: \"fixture1.Test15\"",
            @"Passed: ""fixture1.Test15""
Passed: ""fixture1.Test16""
Passed: ""fixture2.Test25""
Passed: ""fixture2.Test26""",
        };

        [TestCase("Test15", false, 0)]
        [TestCase("Test15", true, 0)]
        [TestCase("Fixture(1|2)\\.Test(15|16|25|26)", false, 1)]
        [TestCase("Fixture(1|2)\\.Test(15|16|25|26)", true, 1)]
        public void WithFilter(string filter, bool useCommandLine, int expectedResultIndex)
        {
            AssetManager.CreateAsset(Properties.Resources.CommandLog, k_CustomCommandPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            TestFixtureUtils.PrepareProjectWithTestFixtures(TestFixtureManager, CommandFactory);
            var logs = TestFixtureUtils.RunTestsAndGetLogs(TestRunner, filter, useCommandLine, TempProjectPath, TestStatusManager.OutputFilePath);

            FileAssert.Exists(TestStatusManager.OutputFilePath);

            Assert.AreEqual(m_Results[expectedResultIndex].FixLineEndings().Trim('\n', '\r', ' '),
                File.ReadAllText(TestStatusManager.OutputFilePath).FixLineEndings().Trim('\n', '\r', ' '));
        }
    }
}
