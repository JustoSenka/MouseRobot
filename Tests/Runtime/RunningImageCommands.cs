using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
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

        private Asset CreateImage(bool isImageSmall)
        {
            return AssetManager.CreateAsset(isImageSmall ? new Bitmap(5, 5) : new Bitmap(15, 15), k_Image);
        }
    }
}
