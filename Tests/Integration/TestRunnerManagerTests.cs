using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot;
using Robot.Abstractions;
using RobotRuntime.IO;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using System.IO;
using System.Linq;
using Unity;

namespace Tests.Integration
{
    [TestClass]
    public class TestRunnerManagerTests
    {
        private string TempProjectPath;

        private const string k_TestAPath = "Tests\\A.mrt";
        private const string k_TestBPath = "Tests\\B.mrt";
        private const string k_TestCPath = "Tests\\C.mrt";
        private const string k_TestDPath = "Tests\\D.mrt";

        IAssetManager AssetManager;
        ITestRunnerManager TestRunnerManager;
        ITestFixtureManager TestFixtureManager;

        [TestInitialize]
        public void Initialize()
        {
            TempProjectPath = TestBase.GenerateProjectPath();
            var container = TestBase.ConstructContainerForTests();

            var ProjectManager = container.Resolve<IProjectManager>();
            AssetManager = container.Resolve<IAssetManager>();
            TestRunnerManager = container.Resolve<ITestRunnerManager>();
            TestFixtureManager = container.Resolve<ITestFixtureManager>();

            ProjectManager.InitProject(TempProjectPath);
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

        [TestMethod]
        public void TestRunnerManager_CorrectlyLoadsTestsFixtures_IfSavedViaTestFixtureManager()
        {
            var fixture = TestFixtureManager.NewTestFixture();
            fixture.ApplyLightFixtureValues(LightTestFixture);
            fixture.AddRecording(new Recording() { Name = "Test" });

            TestFixtureManager.SaveTestFixture(fixture, k_TestAPath);

            Assert.AreEqual(1, TestRunnerManager.TestFixtures.Count, "Only one test fixture should be present");
            Assert.AreEqual(1, TestRunnerManager.TestFixtures[0].Tests.Count, "Only one test should be present");
        }

        [TestMethod]
        public void TestRunnerManager_CorrectlyLoadsTestsFixtures_IfSavedViaAssetManager()
        {
            var lightFixture = LightTestFixture;
            lightFixture.AddRecording(new Recording() { Name = "Test" });

            AssetManager.CreateAsset(lightFixture, k_TestAPath);

            Assert.AreEqual(1, TestRunnerManager.TestFixtures.Count, "Only one test fixture should be present");
            Assert.AreEqual(1, TestRunnerManager.TestFixtures[0].Tests.Count, "Only one test should be present");
        }

        [TestMethod]
        public void TestRunnerManager_CorrectlyLoadsTestsFixtures_IfSavedViaAssetManager_IfTwoFixturesAreSaved()
        {
            var lightFixture = LightTestFixture;
            lightFixture.AddRecording(new Recording() { Name = "Test" });

            AssetManager.CreateAsset(lightFixture, k_TestAPath);
            AssetManager.CreateAsset(lightFixture, k_TestBPath);

            Assert.AreEqual(2, TestRunnerManager.TestFixtures.Count, "Two test fixture should be present");
        }

        [TestMethod]
        public void TestRunnerManager_CorrectlyLoadsTestsFixtures_IfSavedViaFileSystem()
        {
            var lightFixture = LightTestFixture;
            lightFixture.AddRecording(new Recording() { Name = "Test" });

            new YamlTestFixtureIO().SaveObject(k_TestAPath, lightFixture);
            AssetManager.Refresh();

            Assert.AreEqual(1, TestRunnerManager.TestFixtures.Count, "Only one test fixture should be present");
            Assert.AreEqual(1, TestRunnerManager.TestFixtures[0].Tests.Count, "Only one test should be present");
        }

        [TestMethod]
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

        [TestMethod]
        public void TestRunnerManager_CorrectlyHandlesFixtures_WhenRenamingThemOnDisk()
        {
            var lightFixture = LightTestFixture;
            lightFixture.AddRecording(new Recording() { Name = "Test" });

            AssetManager.CreateAsset(lightFixture, k_TestAPath);
            AssetManager.CreateAsset(lightFixture, k_TestBPath);

            File.Move(k_TestAPath, k_TestDPath);
            AssetManager.Refresh();

            Assert.AreEqual(2, TestRunnerManager.TestFixtures.Count, "Two test fixture should be present");
            Assert.AreEqual(k_TestBPath, TestRunnerManager.TestFixtures[0].Path, "Test fixture0 paths missmatched");
            Assert.AreEqual(k_TestDPath, TestRunnerManager.TestFixtures[1].Path, "Test fixture1 paths missmatched");
            Assert.AreEqual("B", TestRunnerManager.TestFixtures[0].Name, "Test fixture0 names missmatched");
            Assert.AreEqual("D", TestRunnerManager.TestFixtures[1].Name, "Test fixture1 names missmatched");
        }
    }
}
