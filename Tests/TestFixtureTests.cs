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

        private const string k_FixturePath = "Tests\\fixture.mrt";

        IAssetManager AssetManager;
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
            TestFixture.ApplyLightScriptValues(LightTestFixture);
            var asset = AssetManager.CreateAsset(TestFixture.ToLightTestFixture(), k_FixturePath);
            Assert.IsTrue(File.Exists(asset.Path), "Test fixture asset should have been created at path: " + k_FixturePath);
        }

        [TestMethod]
        public void TestFixture_FromLightTestFixture_ConvertsCorrectlyBack()
        {
            TestFixture.ApplyLightScriptValues(LightTestFixture);
            var newLightFixture = TestFixture.ToLightTestFixture();
            CheckIfLightTestFixturesAreEqual(LightTestFixture, newLightFixture);
        }

        [TestMethod]
        public void LightTestFixture_LoadedFromImporter_IsCorrect()
        {
            TestFixture.ApplyLightScriptValues(LightTestFixture);
            var asset = AssetManager.CreateAsset(TestFixture.ToLightTestFixture(), k_FixturePath);

            var newLightFixture = asset.Importer.ReloadAsset<LightTestFixture>();
            CheckIfLightTestFixturesAreEqual(LightTestFixture, newLightFixture);
        }

        [TestMethod]
        public void TestFixture_LoadedFromImporter_AndConverted_IsCorrect()
        {
            TestFixture.ApplyLightScriptValues(LightTestFixture);
            var asset = AssetManager.CreateAsset(TestFixture.ToLightTestFixture(), k_FixturePath);

            var newLightFixture = asset.Importer.ReloadAsset<LightTestFixture>();
            var newTestFixture = Container.Resolve<TestFixture>();
            newTestFixture.ApplyLightScriptValues(newLightFixture);

            CheckIfTestFixturesAreEqual(TestFixture, newTestFixture);
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
