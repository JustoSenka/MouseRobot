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

        ILogger Logger;

        private const string k_FixturePath = "Assets\\Fixture.mrt";

        private const string k_CustomCommandPath = "Assets\\CommandLog.cs";
        private const string k_CustomFeatureDetectorPath = "Assets\\Detector.cs";
        private const string k_SmalllImage = "Assets\\small.png";
        private const string k_BigImage = "Assets\\big.png";

        private const string k_DetectorName = "FakeDetector";

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

            ProjectManager.InitProject(TempProjectPath);
        }

        [Test]
        public void IfImageIsVisible(
            [Values(true, false)] bool isImageSmall, 
            [Values(true, false)] bool expectTrue, 
            [Values(true, false)] bool useCommandLine)
        {
            var filter = "Fixture";
            AssetManager.CreateAsset(Properties.Resources.CommandLog, k_CustomCommandPath);
            AssetManager.CreateAsset(Properties.Resources.FeatureDetectorBiggerThan10px, k_CustomFeatureDetectorPath);
            var image = CreateImage(isImageSmall);

            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var f = TestFixtureManager.NewTestFixture();
            var r = new Recording();

            r.AddCommand(TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 1));
            var parentCommand = r.AddCommand(new CommandIfImageVisible(image.Guid, 1000, expectTrue, k_DetectorName));

            r.AddCommand(TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 2), parentCommand);
            r.AddCommand(TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 2), parentCommand);
            f.AddRecording(r);

            TestFixtureManager.SaveTestFixture(f, k_FixturePath);

            var logs = TestFixtureUtils.RunTestsAndGetLogs(TestRunner, filter, useCommandLine, TempProjectPath, TestStatusManager.OutputFilePath);

            var expectedLogcount = (isImageSmall == expectTrue) ? 3 : 1;
            Assert.AreEqual(expectedLogcount, logs.Length, "Log length missmatch");
        }

        private Asset CreateImage(bool isImageSmall)
        {
            return AssetManager.CreateAsset(isImageSmall ? new Bitmap(5, 5) : new Bitmap(15, 15), isImageSmall ? k_SmalllImage : k_BigImage);
        }
    }
}
