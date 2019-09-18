using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity;

namespace Tests.Runtime
{
    [TestFixture]
    public class RunningCustomRecording : TestWithCleanup
    {
        private string TempProjectPath;

        IAssetManager AssetManager;
        IHierarchyManager HierarchyManager;
        IScriptTemplates ScriptTemplates;
        IScriptManager ScriptManager;
        ICommandFactory CommandFactory;
        ITestRunner TestRunner;
        ILogger Logger;

        private const string k_CustomCommandName = "Custom Command";

        private const string k_RecordingName = "Assets\\rec";
        private const string k_RecordingPath = k_RecordingName + ".mrb";
        private const string k_CustomCommandPath = "Assets\\CustomCommand.cs";
        private const string k_CustomCommandRunnerPath = "Assets\\CustomCommandRunner.cs";

        private string ExecutablePath => Path.Combine(Paths.ApplicationInstallPath, "RobotRuntime.exe");

        [SetUp]
        public void Initialize()
        {
            TempProjectPath = TestUtils.GenerateProjectPath();

            var Container = TestUtils.ConstructContainerForTests();

            var ProjectManager = Container.Resolve<IProjectManager>();
            AssetManager = Container.Resolve<IAssetManager>();
            HierarchyManager = Container.Resolve<IHierarchyManager>();
            ScriptManager = Container.Resolve<IScriptManager>();
            ScriptTemplates = Container.Resolve<IScriptTemplates>();
            CommandFactory = Container.Resolve<ICommandFactory>();
            TestRunner = Container.Resolve<ITestRunner>();
            Logger = Container.Resolve<ILogger>();

            TestUtils.InitProjectButDontWaitForScriptCompilation(TempProjectPath, Container);
        }

        [Test]
        public void TestRunner_RunningRecording_FiresCallbacksCorrectly()
        {
            CreateAndSaveTestRecording(k_RecordingPath, out Command c1, out Command c2, out Command c3, CommandFactory);

            var list = new List<Guid>();
            TestRunner.TestData.CommandRunningCallback += (guid) => list.Add(guid);

            TestRunner.LoadSettings();
            TestRunner.StartRecording(k_RecordingName);

            Sync.WaitFor(() => list.Count == 3);
            Assert.IsTrue(list.SequenceEqual(new[] { c1.Guid, c2.Guid, c3.Guid }));
        }

        [Test]
        public void TestRunner_RunningCustomRecording_FiresCallbacksCorrectly()
        {
            PrepareRecordingWith3CustomCommands(out Command c1, out Command c2, out Command c3);

            var list = new List<Guid>();
            TestRunner.TestData.CommandRunningCallback += (guid) => list.Add(guid);

            TestRunner.LoadSettings();
            TestRunner.StartRecording(k_RecordingName);

            Sync.WaitFor(() => list.Count == 3);

            Assert.AreEqual(3, Logger.LogList.Count(log => log.Header == "CommandLog"));
            Assert.AreEqual(3, Logger.LogList.Count(log => log.Header == "RunnerLog"));
            Assert.IsTrue(list.SequenceEqual(new[] { c1.Guid, c2.Guid, c3.Guid }));
        }

        [Test]
        public void TestRunner_RunningCustomRecording_FromDifferentRunnerInSameProcess_Works()
        {
            PrepareRecordingWith3CustomCommands(out Command c1, out Command c2, out Command c3);

            var list = new List<Guid>();

            var container = TestUtils.ConstructContainerForTests();
            container.Resolve<IScriptLoader>(); // Not referenced by runtime
            var logger = container.Resolve<ILogger>();

            var projectManager = container.Resolve<IRuntimeProjectManager>();
            projectManager.InitProject(TempProjectPath).Wait();

            var testRunner = container.Resolve<ITestRunner>();
            testRunner.LoadSettings();

            testRunner.TestData.CommandRunningCallback += (guid) => list.Add(guid);

            testRunner.LoadSettings();
            testRunner.StartRecording(k_RecordingName);

            Sync.WaitFor(() => list.Count == 3);

            Assert.AreEqual(3, logger.LogList.Count(log => log.Header == "CommandLog"));
            Assert.AreEqual(3, logger.LogList.Count(log => log.Header == "RunnerLog"));
            Assert.IsTrue(list.SequenceEqual(new[] { c1.Guid, c2.Guid, c3.Guid }));
        }

        [Test]
        public void CommandLine_RunningCustomRecording_Works()
        {
            PrepareRecordingWith3CustomCommands(out Command c1, out Command c2, out Command c3);

            ScriptManager.AllowCompilation = false;

            var res = ProcessUtility.StartFromCommandLine(ExecutablePath, $"-p {TempProjectPath} -r {k_RecordingName}");
            var logs = FakeLogger.CreateLogsFromConsoleOutput(res);

            Assert.AreEqual(3, logs.Count(log => log.Header == "CommandLog"));
            Assert.AreEqual(3, logs.Count(log => log.Header == "RunnerLog"));
        }

        // This test is not supported.
        // AppDomain contains a lot of unneded assemblies due to many compilations and running command from main method
        // will use incorrect command type, because no Static property provider exists in runtime thus
        // custom commands will be deserialized with random type from domain.
        // Inspecting the created CustomRunner with debugger will throw Internal C# Compiler Exception. This one is also interesting
        // [Test]
        public void Program_RunningCustomRecording_Works()
        {
            TestUtils.CopyAllTemplateScriptsToProjectFolder(ScriptTemplates, AssetManager);

            TestUtils.ReplaceTextInAsset(AssetManager, k_CustomCommandPath, @"// *\[RunnerType", "[RunnerType");
            TestUtils.ReplaceTextInAsset(AssetManager, k_CustomCommandPath, @"// *TODO: RUN METHOD", "Logger.Log(LogType.Log, \"CommandLog\");");
            TestUtils.ReplaceTextInAsset(AssetManager, k_CustomCommandRunnerPath, @"// *TODO: RUN METHOD", "Logger.Log(LogType.Log, \"RunnerLog\");");

            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();
            CreateAndSaveTestRecording(k_RecordingPath, out Command c1, out Command c2, out Command c3, CommandFactory, k_CustomCommandName);

            ScriptManager.AllowCompilation = false;

            RobotRuntime.Program.Main(new[] { Environment.CurrentDirectory, k_RecordingName });
        }

        private void PrepareRecordingWith3CustomCommands(out Command c1, out Command c2, out Command c3)
        {
            TestUtils.CopyAllTemplateScriptsToProjectFolder(ScriptTemplates, AssetManager);

            TestUtils.ReplaceTextInAsset(AssetManager, k_CustomCommandPath, @"// *\[RunnerType", "[RunnerType");
            TestUtils.ReplaceTextInAsset(AssetManager, k_CustomCommandPath, @"// *TODO: RUN METHOD", "Logger.Log(LogType.Log, \"CommandLog\");");
            TestUtils.ReplaceTextInAsset(AssetManager, k_CustomCommandRunnerPath, @"// *TODO: RUN METHOD", "Logger.Log(LogType.Log, \"RunnerLog\");");

            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();
            CreateAndSaveTestRecording(k_RecordingPath, out c1, out c2, out c3, CommandFactory, k_CustomCommandName);
        }

        private Recording CreateAndSaveTestRecording(string name, out Command c1, out Command c2, out Command c3,
            ICommandFactory CommandFactory, string CommandName = "Sleep")
        {
            var r = HierarchyManager.NewRecording();

            c1 = r.AddCommand(CommandFactory.Create(CommandName));
            c2 = r.AddCommand(CommandFactory.Create(CommandName));
            c3 = r.AddCommand(CommandFactory.Create(CommandName));

            HierarchyManager.SaveRecording(r, name);
            return r;
        }
    }
}
