using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using RobotRuntime.Utils.Win32;
using System.Drawing;
using Unity;

namespace Tests.Runtime
{
    [TestFixture]
    public class RunningImageCommands : TestWithCleanup
    {
        private string TempProjectPath;

        ITestFixtureManager TestFixtureManager;
        IAssetManager AssetManager;
        ITestRunner TestRunner;
        IScriptManager ScriptManager;
        ICommandFactory CommandFactory;
        ITestStatusManager TestStatusManager;
        IHierarchyManager HierarchyManager;

        private const string k_FixturePath = "Assets\\Fixture.mrt";

        private const string k_CustomCommandPath = "Assets\\CommandLog.cs";
        private const string k_CustomFeatureDetectorPath = "Assets\\Detector.cs";
        private const string k_Image = "Assets\\image.png";

        private const string k_DetectorWhichOnlyFindsBigImages = "FakeDetector";

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
            HierarchyManager = Container.Resolve<IHierarchyManager>();

            ProjectManager.InitProject(TempProjectPath);
        }

        [Test]
        public void IfImageIsVisible(
            [Values(true, false)] bool isImageSmall,
            [Values(true, false)] bool runCommandsIfImageWasFound,
            [Values(true, false)] bool useCommandLine)
        {
            AssetManager.CreateAsset(Properties.Resources.CommandLog, k_CustomCommandPath);
            AssetManager.CreateAsset(Properties.Resources.BigImageDetectorFake, k_CustomFeatureDetectorPath);
            var image = CreateImage(isImageSmall);

            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var f = TestFixtureManager.NewTestFixture();
            var r = new Recording();

            r.AddCommand(TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 1));
            var parentCommand = r.AddCommand(new CommandIfImageVisible(image.Guid, 1000, runCommandsIfImageWasFound, k_DetectorWhichOnlyFindsBigImages));

            r.AddCommand(TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 2), parentCommand);
            r.AddCommand(TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 2), parentCommand);
            f.AddRecording(r);

            TestFixtureManager.SaveTestFixture(f, k_FixturePath);

            var logs = TestFixtureUtils.RunTestsAndGetLogs(TestRunner, ".", useCommandLine, TempProjectPath, TestStatusManager.OutputFilePath);

            var expectedLogcount = runCommandsIfImageWasFound != isImageSmall ? 3 : 1;
            Assert.AreEqual(expectedLogcount, logs.Length, "Log length missmatch");
        }

        private const string k_RecordingPath = "Assets\\rec.mrb";

        [Test]
        public void MoveMouseToImage([Values(true, false)] bool useCommandLine)
        {
            AssetManager.CreateAsset(Properties.Resources.CommandLog, k_CustomCommandPath);
            AssetManager.CreateAsset(Properties.Resources.BigImageDetectorFake, k_CustomFeatureDetectorPath);
            var image = CreateImage(false);

            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();
            var positionBefore = WinAPI.GetCursorPosition();

            var r = HierarchyManager.NewRecording();
            r.AddCommand(TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 2));
            var parentCommand = r.AddCommand(new CommandForImage(image.Guid, 1000, false, k_DetectorWhichOnlyFindsBigImages));
            r.AddCommand(new CommandMove(0, 0), parentCommand);
            HierarchyManager.SaveRecording(r, k_RecordingPath);

            var logs = TestFixtureUtils.RunRecordingAndGetLogs(TestRunner, k_RecordingPath, useCommandLine, TempProjectPath, TestStatusManager.OutputFilePath);
            var currentPos = WinAPI.GetCursorPosition();
            WinAPI.SetCursorPosition(positionBefore); //Reset cursor position to original

            Assert.AreEqual(1, logs.Length, "Log length missmatch");
            TestFixtureUtils.AssertIfCommandLogsAreOutOfOrder(logs, 2);

            Assert.AreEqual(new Point(15, 15), currentPos, "Mouse was in incorrect position");
        }

        private Asset CreateImage(bool isImageSmall)
        {
            return AssetManager.CreateAsset(isImageSmall ? new Bitmap(5, 5) : new Bitmap(15, 15), k_Image);
        }
    }
}
