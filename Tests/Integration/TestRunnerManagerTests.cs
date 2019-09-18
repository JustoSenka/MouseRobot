using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using RobotRuntime.Abstractions;
using RobotRuntime.IO;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using System.IO;
using System.Linq;
using Unity;

namespace Tests.Integration
{
    [TestFixture]
    public class TestRunnerManagerTests : TestWithCleanup
    {
        private string TempProjectPath;

        private const string k_TestAPath = "Assets\\A.mrt";
        private const string k_TestBPath = "Assets\\B.mrt";
        private const string k_TestCPath = "Assets\\C.mrt";
        private const string k_TestDPath = "Assets\\D.mrt";

        private const string k_TestFolderPath = "Assets\\TestFolder";
        private const string k_FixtureInFolder= "Assets\\TestFolder\\fix.mrt";

        private const string k_NewTestFolderPath = "Assets\\TestFolder-new";
        private const string k_FixtureInNewFolder = "Assets\\TestFolder-new\\fix.mrt";

        IAssetManager AssetManager;
        ITestRunnerManager TestRunnerManager;
        ITestFixtureManager TestFixtureManager;

        [SetUp]
        public void Initialize()
        {
            TempProjectPath = TestUtils.GenerateProjectPath();
            var container = TestUtils.ConstructContainerForTests();

            var ProjectManager = container.Resolve<IProjectManager>();
            AssetManager = container.Resolve<IAssetManager>();
            TestRunnerManager = container.Resolve<ITestRunnerManager>();
            TestFixtureManager = container.Resolve<ITestFixtureManager>();
            var ScriptLoader = container.Resolve<IScriptLoader>();

            TestUtils.InitProjectButDontWaitForScriptCompilation(TempProjectPath, container);

            // Will invoke domain reloaded callback which will tell TestRunnerManager to load fixtures
            // It is faster than ScriptManager.Recompile... so loading directly even if there is nothing to load
            // Just for tests
            ScriptLoader.CreateUserAppDomain(); 
        }

        private LightTestFixture LightTestFixture
        {
            get
            {
                var f = new LightTestFixture();
                f.Name = "TestName";
                f.Setup = new Recording();
                f.TearDown = new Recording();
                f.OneTimeSetup = new Recording();
                f.OneTimeTeardown = new Recording();
                f.Setup.Name = LightTestFixture.k_Setup;
                f.TearDown.Name = LightTestFixture.k_TearDown;
                f.OneTimeSetup.Name = LightTestFixture.k_OneTimeSetup;
                f.OneTimeTeardown.Name = LightTestFixture.k_OneTimeTeardown;
                f.Tests = new Recording[] { }.ToList();
                return f;
            }
        }

        [Test]
        public void TestRunnerManager_CorrectlyLoadsTestsFixtures_IfSavedViaTestFixtureManager()
        {
            var fixture = TestFixtureManager.NewTestFixture();
            fixture.ApplyLightFixtureValues(LightTestFixture);
            fixture.AddRecording(new Recording() { Name = "Test" });

            TestFixtureManager.SaveTestFixture(fixture, k_TestAPath);

            Assert.AreEqual(1, TestRunnerManager.TestFixtures.Count, "Only one test fixture should be present");
            Assert.AreEqual(1, TestRunnerManager.TestFixtures[0].Tests.Count, "Only one test should be present");
        }

        [Test]
        public void TestRunnerManager_CorrectlyLoadsTestsFixtures_IfSavedViaAssetManager()
        {
            var lightFixture = LightTestFixture;
            lightFixture.AddRecording(new Recording() { Name = "Test" });

            AssetManager.CreateAsset(lightFixture, k_TestAPath);

            Assert.AreEqual(1, TestRunnerManager.TestFixtures.Count, "Only one test fixture should be present");
            Assert.AreEqual(1, TestRunnerManager.TestFixtures[0].Tests.Count, "Only one test should be present");
        }

        [Test]
        public void TestRunnerManager_CorrectlyLoadsTestsFixtures_IfSavedViaAssetManager_IfTwoFixturesAreSaved()
        {
            var lightFixture = LightTestFixture;
            lightFixture.AddRecording(new Recording() { Name = "Test" });

            AssetManager.CreateAsset(lightFixture, k_TestAPath);
            AssetManager.CreateAsset(lightFixture, k_TestBPath);

            Assert.AreEqual(2, TestRunnerManager.TestFixtures.Count, "Two test fixture should be present");
        }

        [Test]
        public void TestRunnerManager_CorrectlyLoadsTestsFixtures_IfSavedViaFileSystem()
        {
            var lightFixture = LightTestFixture;
            lightFixture.AddRecording(new Recording() { Name = "Test" });

            new YamlTestFixtureIO().SaveObject(k_TestAPath, lightFixture);
            AssetManager.Refresh();

            Assert.AreEqual(1, TestRunnerManager.TestFixtures.Count, "Only one test fixture should be present");
            Assert.AreEqual(1, TestRunnerManager.TestFixtures[0].Tests.Count, "Only one test should be present");
        }

        [Test]
        public void TestRunnerManager_CorrectlyRemovesFixtures_WhenDeletingThemOnDisk()
        {
            var lightFixture = LightTestFixture;
            lightFixture.AddRecording(new Recording() { Name = "Test" });

            AssetManager.CreateAsset(lightFixture, k_TestAPath);
            AssetManager.CreateAsset(lightFixture, k_TestBPath);
            AssetManager.CreateAsset(lightFixture, k_TestCPath);

            AssetManager.DeleteAsset(k_TestAPath);
            AssetManager.DeleteAsset(k_TestBPath);

            Assert.AreEqual(1, TestRunnerManager.TestFixtures.Count, "Only one test fixture should be present");
            Assert.AreEqual(k_TestCPath, TestRunnerManager.TestFixtures[0].Path, "Test fixture paths missmatched");
            Assert.AreEqual("C", TestRunnerManager.TestFixtures[0].Name, "Test fixture names missmatched");
        }

        [Test]
        public void TestRunnerManager_CorrectlyHandlesFixtures_WhenRenamingThemOnDisk()
        {
            var lightFixture = LightTestFixture;
            lightFixture.AddRecording(new Recording() { Name = "Test" });

            AssetManager.CreateAsset(lightFixture, k_TestAPath);
            AssetManager.CreateAsset(lightFixture, k_TestBPath);

            File.Move(k_TestAPath, k_TestDPath);
            AssetManager.Refresh();

            Assert.AreEqual(2, TestRunnerManager.TestFixtures.Count, "Two test fixture should be present");
            CollectionAssert.AreEquivalent(new[] { k_TestBPath, k_TestDPath }, TestRunnerManager.TestFixtures.Select(f => f.Path), "Fixture Paths in TestRunnerManager are incorrect");
            CollectionAssert.AreEquivalent(new[] { "B", "D" }, TestRunnerManager.TestFixtures.Select(f => f.Name), "Fixture Names in TestRunnerManager are incorrect");
        }

        [Test]
        public void DeletingFolder_WithTestFixtures_CorrectlyRemovesFixturesFromTestRunner()
        {
            var lightFixture = LightTestFixture;
            lightFixture.AddRecording(new Recording() { Name = "Test" });

            AssetManager.CreateAsset(null, k_TestFolderPath);
            AssetManager.CreateAsset(lightFixture, k_TestAPath);
            AssetManager.CreateAsset(lightFixture, k_FixtureInFolder);

            Assert.AreEqual(2, TestRunnerManager.TestFixtures.Count, "Two test fixture should be present before deletion");

            AssetManager.DeleteAsset(k_TestFolderPath);

            Assert.AreEqual(1, TestRunnerManager.TestFixtures.Count, "Only one fixture should be present after deletion");
            Assert.AreEqual(k_TestAPath, TestRunnerManager.TestFixtures[0].Path, "Test fixture0 paths missmatched");
        }

        [Test]
        public void RenamingFolder_ShouldUpdateFixturePath_InTestRunnerManager()
        {
            var lightFixture = LightTestFixture;
            lightFixture.AddRecording(new Recording() { Name = "Test" });

            AssetManager.CreateAsset(null, k_TestFolderPath);
            AssetManager.CreateAsset(lightFixture, k_FixtureInFolder);

            AssetManager.RenameAsset(k_TestFolderPath, k_NewTestFolderPath);

            Assert.AreEqual(k_FixtureInNewFolder, TestRunnerManager.TestFixtures[0].Path, "Test fixture0 paths missmatched");
        }
    }
}
