using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using RobotRuntime.Utils;
using System;
using System.IO;
using System.Threading.Tasks;
using Unity;

namespace Tests.Integration
{
    [TestFixture]
    public class CustomCommandCompilationTests : TestWithCleanup
    {
        private string TempProjectPath;

        IAssetManager AssetManager;
        IHierarchyManager HierarchyManager;
        IScriptTemplates ScriptTemplates;
        IScriptManager ScriptManager;
        ITestFixtureManager TestFixtureManager;
        ICommandFactory CommandFactory;
        IScriptLoader ScriptLoader;

        private const string k_CustomCommand = "Custom Command";

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
            TestFixtureManager = container.Resolve<ITestFixtureManager>();
            ScriptLoader = container.Resolve<IScriptLoader>();

            ProjectManager.InitProjectNoScriptCompile(TempProjectPath);
            TestUtils.CopyAllTemplateScriptsToProjectFolder(ScriptTemplates, AssetManager);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();
        }

        [Test]
        public void ScriptTemplates_ShouldNotHave_CompilationErrors()
        {
            var compileSucceeded1 = ScriptManager.CompileScriptsAndReloadUserDomain().Result;
            var compileSucceeded2 = ScriptManager.CompileScriptsAndReloadUserDomain().Result;

            Assert.IsTrue(compileSucceeded1, "Script templates have compilation errors 1");
            Assert.IsTrue(compileSucceeded2, "Script templates have compilation errors 2");
        }

        [Test]
        public void ScriptTemplates_Should_ProduceBothDllAndPdbFiles()
        {
            var dllPath = ScriptLoader.UserAssemblyPath;
            var pdbPath = dllPath.Replace(".dll", ".pdb");
            
            Assert.IsTrue(File.Exists(dllPath), "Dll path is missing: " + dllPath);
            Assert.IsTrue(File.Exists(pdbPath), "Pdb path is missing: " + pdbPath);
        }

        [Test]
        public void CommandFactory_CanCreate_CustomCommand()
        {
            var command = CommandFactory.Create(k_CustomCommand);
            Assert.AreEqual(k_CustomCommand, command.Name, "Command names differ");
            Assert.AreEqual(5, ((dynamic)command).SomeInt, "Command some int default value differs");
        }

        [Test]
        public void CommandFactory_UpdatesCommandType_AfterRecompile()
        {
            var oldCommandType = CommandFactory.Create(k_CustomCommand).GetType();

            TestUtils.ModifyScriptAsset(AssetManager, "Assets\\CustomCommand.cs");
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var newCommandType = CommandFactory.Create(k_CustomCommand).GetType();
            Assert.AreNotEqual(oldCommandType, newCommandType, "CommandFactory should give different commands after recompilation");
        }

        [Test]
        public void CustomCommand_InsideLoadedRecordingIsUpdated_AfterRecompile()
        {
            var oldCommand = CommandFactory.Create(k_CustomCommand);
            var rec = HierarchyManager.NewRecording();
            rec.AddCommand(oldCommand);

            TestUtils.ModifyScriptAsset(AssetManager, "Assets\\CustomCommand.cs");
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var newCommand = rec.Commands.GetChild(0).value;
            Assert.AreNotEqual(oldCommand.GetType(), newCommand.GetType(), "Commands in recordings loded in hierarchy should be replaced with new one after recompilation");
        }

        [Test]
        public void CustomCommand_SerializingAndDeserializing_KeepsItsType()
        {
            SubscribeToEventAndAssertThatNoDomainReloadsAreHappeningAfterThisPoint();

            var oldCommand = CommandFactory.Create(k_CustomCommand);
            var oldRec = HierarchyManager.NewRecording();
            oldRec.AddCommand(oldCommand);

            var recPath = Path.Combine(Paths.AssetsPath, "rec.mrb");
            HierarchyManager.SaveRecording(oldRec, recPath);

            var newRec = new Asset(recPath).Load<Recording>();
            var newCommand = newRec.Commands.GetChild(0).value;

            Assert.AreEqual(oldCommand.GetType(), newCommand.GetType(), "Command type after serialization and deserialization should not become different");
        }

        [Test]
        public void CustomCommand_InsideTestFixtureIsUpdates_AdterRecompile()
        {
            var testFixture = TestFixtureManager.NewTestFixture();
            var oldCommand = CommandFactory.Create(k_CustomCommand);
            testFixture.Setup.AddCommand(oldCommand);

            TestUtils.ModifyScriptAsset(AssetManager, "Assets\\CustomCommand.cs");
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var newCommand = testFixture.Setup.Commands.GetChild(0).value;

            Assert.AreNotEqual(oldCommand.GetType(), newCommand.GetType(), "Commands in loaded test fixture should be replaced with new onw after recompilation");
        }

        [Test]
        public void CustomCommand_SerializingAndDeserializing_InsideTestFixture_KeepsItsType()
        {
            CommandFactory.NewUserCommands += () => Console.WriteLine("New Commands Appeared");

            SubscribeToEventAndAssertThatNoDomainReloadsAreHappeningAfterThisPoint();

            var testFixture = TestFixtureManager.NewTestFixture();
            var oldCommand = CommandFactory.Create(k_CustomCommand);
            testFixture.Setup.AddCommand(oldCommand);

            var testPath = Path.Combine(Paths.AssetsPath, "test.mrt");
            TestFixtureManager.SaveTestFixture(testFixture, testPath);

            var newTestFixture = new Asset(testPath).Load<LightTestFixture>();
            var newCommand = newTestFixture.Setup.Commands.GetChild(0).value;

            Assert.AreEqual(oldCommand.GetType(), newCommand.GetType(), "Command type after serialization and deserialization should not become different");
        }

        [Test]
        public void CustomCommand_CreatedFromFactory_AlwaysHasSameType()
        {
            var testFixture = TestFixtureManager.NewTestFixture();
            var c1 = CommandFactory.Create(k_CustomCommand);
            var c2 = CommandFactory.Create(k_CustomCommand);

            Assert.AreEqual(c1.GetType(), c2.GetType(), "CommandFactory should always give same type commands");
        }

        [Test]
        public void CustomCommand_KeepsItsType_WhileBeingSavedInOtherAssets()
        {
            SubscribeToEventAndAssertThatNoDomainReloadsAreHappeningAfterThisPoint();

            var testFixture = TestFixtureManager.NewTestFixture();
            var oldCommand = CommandFactory.Create(k_CustomCommand);
            testFixture.Setup.AddCommand(oldCommand);

            TestFixtureManager.SaveTestFixture(testFixture, Path.Combine(Paths.AssetsPath, "test.mrt"));
            TestFixtureManager.SaveTestFixture(testFixture, Path.Combine(Paths.AssetsPath, "test.mrt"));
            TestFixtureManager.SaveTestFixture(testFixture, Path.Combine(Paths.AssetsPath, "test.mrt"));

            var sameCommand = testFixture.Setup.Commands.GetChild(0).value;

            Assert.AreEqual(oldCommand.GetType(), sameCommand.GetType(), "Command type should not change due to it being saved in other assets");
        }

        private void SubscribeToEventAndAssertThatNoDomainReloadsAreHappeningAfterThisPoint()
        {
            CommandFactory.NewUserCommands += () =>
            {
                // This test showed signs of instability once, leaving here for future reference
                Console.WriteLine("New Commands Appeared. This should never be called, if it is called, we failed to correctly wait for all tasks to finish.");
                Assert.Fail();
            };
        }

        [Test]
        public void NativeCommand_SerializingAndDeserializing_InsideTestFixture_KeepsItsType()
        {
            var testFixture = TestFixtureManager.NewTestFixture();
            var oldCommand = CommandFactory.Create(new CommandMove().Name);
            testFixture.Setup.AddCommand(oldCommand);

            var testPath = Path.Combine(Paths.AssetsPath, "test.mrt");
            TestFixtureManager.SaveTestFixture(testFixture, testPath);

            var newTestFixture = new Asset(testPath).Load<LightTestFixture>();
            var newCommand = newTestFixture.Setup.Commands.GetChild(0).value;

            Assert.AreEqual(oldCommand.GetType(), newCommand.GetType(), "Command type after serialization and deserialization should not become different");
        }
    }
}
