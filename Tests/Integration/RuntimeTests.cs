using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot;
using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Recordings;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity;

namespace Tests.Integration
{
    [TestClass]
    public class RuntimeTests
    {
        private string TempProjectPath;

        IMouseRobot MouseRobot;
        IAssetManager AssetManager;
        IHierarchyManager HierarchyManager;
        IScriptTemplates ScriptTemplates;
        IScriptCompiler ScriptCompiler;
        IScriptManager ScriptManager;
        ITestFixtureManager TestFixtureManager;
        ICommandFactory CommandFactory;
        ITestRunner TestRunner;

        private const string k_CustomCommandName = "Custom Command";

        private const string k_RecordingName = "rec";
        private const string k_RecordingPath = "Recordings\\" + k_RecordingName + ".mrb";
        private const string k_CustomCommandPath = "Scripts\\CustomCommand.cs";
        private const string k_CustomCommandRunnerPath = "Scripts\\CustomCommandRunner.cs";

        [TestInitialize]
        public void Initialize()
        {
            TempProjectPath = TestBase.GenerateProjectPath();
            
            var container = TestBase.ConstructContainerForTests();

            MouseRobot = container.Resolve<IMouseRobot>();
            var ProjectManager = container.Resolve<IProjectManager>();
            AssetManager = container.Resolve<IAssetManager>();
            HierarchyManager = container.Resolve<IHierarchyManager>();
            ScriptManager = container.Resolve<IScriptManager>();
            ScriptCompiler = container.Resolve<IScriptCompiler>();
            ScriptTemplates = container.Resolve<IScriptTemplates>();
            CommandFactory = container.Resolve<ICommandFactory>();
            TestFixtureManager = container.Resolve<ITestFixtureManager>();
            TestRunner = container.Resolve<ITestRunner>();

            ProjectManager.InitProject(TempProjectPath);
        }

        [TestMethod]
        public void RunningRecording_FromTestRunner_FiresCallbacksCorrectly()
        {
            CreateAndSaveTestRecording(k_RecordingPath, out Command c1, out Command c2, out Command c3, CommandFactory);

            var list = new List<Guid>();
            TestRunner.TestData.CommandRunningCallback += (guid) => list.Add(guid);

            TestRunner.LoadSettings();
            TestRunner.StartRecording(k_RecordingName);

            Sync.WaitFor(() => list.Count == 3);
            Assert.IsTrue(list.SequenceEqual(new[] { c1.Guid, c2.Guid, c3.Guid }));
        }

        [TestMethod]
        public void RunningCustomRecording_FromCommandLine_Works()
        {
            /*TestBase.CopyAllTemplateScriptsToProjectFolder(ScriptTemplates, AssetManager);

            TestBase.ReplaceTextInAsset(AssetManager, k_CustomCommandPath, @"// *\[RunnerType", "[RunnerType");
            TestBase.ReplaceTextInAsset(AssetManager, k_CustomCommandPath, @"// *TODO: RUN METHOD", "Logger.Log(LogType.Log, \"CommandLog\");");
            TestBase.ReplaceTextInAsset(AssetManager, k_CustomCommandRunnerPath, @"// *TODO: RUN METHOD", "Logger.Log(LogType.Log, \"RunnerLog\");");

            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();
            CreateAndSaveTestRecording(k_RecordingPath, out Command c1, out Command c2, out Command c3, CommandFactory, k_CustomCommandName);

            var res = ProcessUtility.StartFromCommandLine(
                Path.Combine(Paths.ApplicationInstallPath, "RobotRuntime.exe"), 
                TempProjectPath + " " + k_RecordingName);

            RobotRuntime.Program.Main(new[] { Environment.CurrentDirectory, k_RecordingName });
            Console.WriteLine(res);*/
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
