using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using RobotRuntime.Abstractions;
using RobotRuntime.Tests;
using Unity;

namespace Tests.Runtime
{
    [TestFixture]
    public class RunningCommandsFromDll : TestWithCleanup
    {
        private string TempProjectPath;

        IAssetManager AssetManager;
        IHierarchyManager HierarchyManager;
        IScriptManager ScriptManager;
        ICommandFactory CommandFactory;
        ITestRunner TestRunner;
        ITestStatusManager TestStatusManager;

        private const string k_UserDllName = "LibWithCommand";
        private const string k_UserDllPath = "Assets\\" + k_UserDllName + ".dll";
        private const string k_RecordingPath = "Assets\\rec.mrb";
        private const string k_SomeScript = "Assets\\SomeScript.cs";

        [SetUp]
        public void Initialize()
        {
            TempProjectPath = TestUtils.GenerateProjectPath();

            var Container = TestUtils.ConstructContainerForTests();

            var ProjectManager = Container.Resolve<IProjectManager>();
            AssetManager = Container.Resolve<IAssetManager>();
            HierarchyManager = Container.Resolve<IHierarchyManager>();
            ScriptManager = Container.Resolve<IScriptManager>();
            CommandFactory = Container.Resolve<ICommandFactory>();
            TestRunner = Container.Resolve<ITestRunner>();
            TestStatusManager = Container.Resolve<ITestStatusManager>();

            ProjectManager.InitProjectNoScriptCompile(TempProjectPath);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void RunRecording_WithCommand_FromPlugin(bool useCommandLine)
        {
            AssetManager.CreateAsset(Properties.Resources.TestClassLibraryWithRefToRobot, k_UserDllPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var r = HierarchyManager.NewRecording();
            r.AddCommand(TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 14));
            r.AddCommand(TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 18));
            HierarchyManager.SaveRecording(r, k_RecordingPath);

            var logs = TestFixtureUtils.RunRecordingAndGetLogs(TestRunner, k_RecordingPath, useCommandLine, TempProjectPath, TestStatusManager.OutputFilePath);
            TestFixtureUtils.AssertIfCommandLogsAreOutOfOrder(logs, 14, 18);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Recordings_CanExecuteCode_FromUserPlugins(bool useCommandLine)
        {
            AssetManager.CreateAsset(Properties.Resources.TestClassLibrary, k_UserDllPath);
            AssetManager.CreateAsset(Properties.Resources.CommandUsingUserPlugin, k_SomeScript);
            var res = ScriptManager.CompileScriptsAndReloadUserDomain().Result;

            var r = HierarchyManager.NewRecording();
            r.AddCommand(TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 0));
            r.AddCommand(TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 8));
            HierarchyManager.SaveRecording(r, k_RecordingPath);

            var logs = TestFixtureUtils.RunRecordingAndGetLogs(TestRunner, k_RecordingPath, useCommandLine, TempProjectPath, TestStatusManager.OutputFilePath);
            TestFixtureUtils.AssertIfCommandLogsAreOutOfOrder(logs, 96, 104); // 96 comes from user dll + number we've given the command
        }
    }
}
