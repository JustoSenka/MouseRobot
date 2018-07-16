using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Robot;
using System.IO;
using Robot.Abstractions;
using Unity;
using RobotRuntime.Utils;
using Unity.Lifetime;
using RobotRuntime.Abstractions;
using RobotRuntime;
using RobotRuntime.Scripts;
using RobotRuntime.Tests;
using Robot.Tests;

namespace Tests
{
    [TestClass]
    public class TestFixtureTests
    {
        private string TempProjectPath
        {
            get
            {
                return Path.GetTempPath() + "\\MProject";
            }
        }

        private const string k_FixtureName = "fixture";
        private const string k_FixturePath = "Tests\\" + k_FixtureName + ".mrt";

        IAssetManager AssetManager;
        private ITestFixtureManager TestFixtureManager;
        UnityContainer Container;

        private TestFixture TestFixture;

        private LightTestFixture LightTestFixture
        {
            get
            {
                var f = new LightTestFixture();
                f.Name = "TestName";
                f.Setup = new Script();
                f.TearDown = new Script();
                f.OneTimeSetup = new Script();
                f.OneTimeTeardown = new Script();
                f.Setup.Name = LightTestFixture.k_Setup;
                f.TearDown.Name = LightTestFixture.k_TearDown;
                f.OneTimeSetup.Name = LightTestFixture.k_OneTimeSetup;
                f.OneTimeTeardown.Name = LightTestFixture.k_OneTimeTeardown;
                f.Tests = new Script[] { new Script(), new Script() }.ToList();
                return f;
            }
        }

        [TestMethod]
        public void AssetIsCreated_For_LightTestFixture()
        {
            var asset = AssetManager.CreateAsset(LightTestFixture, k_FixturePath);
            Assert.IsTrue(File.Exists(asset.Path), "Test fixture asset should have been created at path: " + k_FixturePath);
        }

        [TestMethod]
        public void AssetIsCreated_For_TestFixture()
        {
            TestFixture.ApplyLightFixtureValues(LightTestFixture);
            var asset = AssetManager.CreateAsset(TestFixture.ToLightTestFixture(), k_FixturePath);
            Assert.IsTrue(File.Exists(asset.Path), "Test fixture asset should have been created at path: " + k_FixturePath);
        }

        [TestMethod]
        public void TestFixture_FromLightTestFixture_ConvertsCorrectlyBack()
        {
            TestFixture.ApplyLightFixtureValues(LightTestFixture);
            var newLightFixture = TestFixture.ToLightTestFixture();
            CheckIfLightTestFixturesAreEqual(LightTestFixture, newLightFixture);
        }

        [TestMethod]
        public void LightTestFixture_LoadedFromImporter_IsCorrect()
        {
            TestFixture.ApplyLightFixtureValues(LightTestFixture);
            var asset = AssetManager.CreateAsset(TestFixture.ToLightTestFixture(), k_FixturePath);

            var newLightFixture = asset.Importer.ReloadAsset<LightTestFixture>();
            CheckIfLightTestFixturesAreEqual(LightTestFixture, newLightFixture);
        }

        [TestMethod]
        public void TestFixture_LoadedFromImporter_AndConverted_IsCorrect()
        {
            TestFixture.ApplyLightFixtureValues(LightTestFixture);
            var asset = AssetManager.CreateAsset(TestFixture.ToLightTestFixture(), k_FixturePath);

            var newLightFixture = asset.Importer.ReloadAsset<LightTestFixture>();
            var newTestFixture = Container.Resolve<TestFixture>();
            newTestFixture.ApplyLightFixtureValues(newLightFixture);

            CheckIfTestFixturesAreEqual(TestFixture, newTestFixture);
        }

        [TestMethod]
        public void SavingFixtureVia_TestFixtureManager_ProducesCorrectAsset()
        {
            var fixture = TestFixtureManager.NewTestFixture();
            var lightFixture = LightTestFixture;
            fixture.ApplyLightFixtureValues(lightFixture);
            TestFixtureManager.SaveTestFixture(fixture, k_FixturePath);

            var asset = AssetManager.GetAsset(k_FixturePath);
            var newLightFixture = asset.Importer.ReloadAsset<LightTestFixture>();
            newLightFixture.Name = lightFixture.Name; // Names are overrided by path they are saved

            CheckIfLightTestFixturesAreEqual(lightFixture, newLightFixture);
        }

        [TestMethod]
        public void SavingFixture_ToPath_WillAdjustTheNameOfFixture()
        {
            var fixture = TestFixtureManager.NewTestFixture();
            var lightFixture = LightTestFixture;
            fixture.ApplyLightFixtureValues(lightFixture);

            TestFixtureManager.SaveTestFixture(fixture, k_FixturePath);

            Assert.AreEqual(k_FixtureName, fixture.Name, "Names should be adjusted according to path");
        }

        [TestMethod]
        public void ApplyLightFixtureValues_CorrectlyAppliesFixtureNames()
        {
            var fixture = TestFixtureManager.NewTestFixture();
            var lightFixture = LightTestFixture;
            fixture.ApplyLightFixtureValues(lightFixture);

            Assert.AreEqual(lightFixture.Name, fixture.Name, "Names should be the same");
        }

        private void CheckIfLightTestFixturesAreEqual(LightTestFixture a, LightTestFixture b)
        {
            Assert.AreEqual(a.Name, b.Name, "Names missmatched");
            Assert.AreEqual(a.Setup.Name, b.Setup.Name, "Setup names missmatched");
            Assert.AreEqual(a.OneTimeTeardown.Name, b.OneTimeTeardown.Name, "Teardown names missmatched");
            Assert.AreEqual(a.Setup.Commands.Count(), b.Setup.Commands.Count(), "Setup command count missmatched");
            Assert.AreEqual(a.OneTimeTeardown.Commands.Count(), b.OneTimeTeardown.Commands.Count(), "teardown command count missmatched");
            Assert.AreEqual(a.Tests.Count, b.Tests.Count, "Test count missmatched");
        }

        private void CheckIfTestFixturesAreEqual(TestFixture a, TestFixture b)
        {
            Assert.AreEqual(a.Name, b.Name, "Names missmatched");
            Assert.AreEqual(a.Setup.Name, b.Setup.Name, "Setup names missmatched");
            Assert.AreEqual(a.OneTimeTeardown.Name, b.OneTimeTeardown.Name, "Teardown names missmatched");
            Assert.AreEqual(a.Setup.Commands.Count(), b.Setup.Commands.Count(), "Setup command count missmatched");
            Assert.AreEqual(a.OneTimeTeardown.Commands.Count(), b.OneTimeTeardown.Commands.Count(), "teardown command count missmatched");
            Assert.AreEqual(a.Tests.Count, b.Tests.Count, "Test count missmatched");
        }

        [TestInitialize]
        public void Initialize()
        {
            Container = new UnityContainer();
            RobotRuntime.Program.RegisterInterfaces(Container);
            Robot.Program.RegisterInterfaces(Container);

            Container.RegisterType<ILogger, FakeLogger>(new ContainerControlledLifetimeManager());
            Logger.Instance = Container.Resolve<ILogger>();

            var ProjectManager = Container.Resolve<IProjectManager>();
            AssetManager = Container.Resolve<IAssetManager>();
            TestFixtureManager = Container.Resolve<ITestFixtureManager>();
            TestFixture = Container.Resolve<TestFixture>();

            ProjectManager.InitProject(TempProjectPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            DirectoryInfo di = new DirectoryInfo(TempProjectPath + "\\" + Paths.TestsFolder);
            foreach (FileInfo file in di.GetFiles())
                file.Delete();

            AssetManager.Refresh();
        }
    }
}
