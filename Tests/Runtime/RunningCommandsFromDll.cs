using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity;

namespace Tests.Runtime
{
    [TestFixture]
    public class RunningCommandsFromDll : TestWithCleanup
    {
        private string TempProjectPath;

        IAssetManager AssetManager;
        IHierarchyManager HierarchyManager;
        IScriptTemplates ScriptTemplates;
        IScriptManager ScriptManager;
        ICommandFactory CommandFactory;
        ITestRunner TestRunner;
        ITestStatusManager TestStatusManager;
        ILogger Logger;

        private const string k_UserDllPath = "Assets\\SomeDll.dll";

        private string ExecutablePath => Path.Combine(Paths.ApplicationInstallPath, "RobotRuntime.exe");

        [SetUp]
        public void Initialize()
        {
            TempProjectPath = TestUtils.GenerateProjectPath();

            var container = TestUtils.ConstructContainerForTests();

            var ProjectManager = container.Resolve<IProjectManager>();
            AssetManager = container.Resolve<IAssetManager>();
            HierarchyManager = container.Resolve<IHierarchyManager>();
            ScriptManager = container.Resolve<IScriptManager>();
            ScriptTemplates = container.Resolve<IScriptTemplates>();
            CommandFactory = container.Resolve<ICommandFactory>();
            TestRunner = container.Resolve<ITestRunner>();
            TestStatusManager = container.Resolve<ITestStatusManager>();
            Logger = container.Resolve<ILogger>();

            ProjectManager.InitProject(TempProjectPath);
        }

        [Test]
        public void RunRecording_WithCommand_FromPlugin([Values(true, false)] bool useCommandLine)
        {
            AssetManager.CreateAsset(Properties.Resources.TestClassLibraryWithRefToRobot, k_UserDllPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();


            /*
            var logs = TestFixtureUtils.RunTestsAndGetLogs(TestRunner, ".", useCommandLine, TempProjectPath, TestStatusManager.OutputFilePath);

            Assert.AreEqual(2, logs.Length, "Log length missmatch");*/
        }
    }
}
