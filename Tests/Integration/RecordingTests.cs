using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using Robot.Tests;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using System.Linq;
using Unity;

namespace Tests.Integration
{
    [TestFixture]
    public class RecordingTests : TestWithCleanup
    {
        private string TempProjectPath;

        IAssetManager AssetManager;
        IHierarchyManager HierarchyManager;
        IScriptManager ScriptManager;
        ICommandFactory CommandFactory;
        ITestRunner TestRunner;
        ITestStatusManager TestStatusManager;
        ITestFixtureManager TestFixtureManager;
        ITestRunnerManager TestRunnerManager;

        private const string k_UserDllName = "LibWithCommand";
        private const string k_UserDllPath = "Assets\\" + k_UserDllName + ".dll";
        private const string k_UserDllNewPath = "Assets\\NewName" + k_UserDllName + ".dll";
        private const string k_RecordingPath = "Assets\\rec.mrb";
        private const string k_FixturePath = "Assets\\fix.mrt";
        private const string k_SomeScript = "Assets\\SomeScript.cs";

        [SetUp]
        public void Initialize()
        {
            TempProjectPath = TestUtils.GenerateProjectPath();

            var container = TestUtils.ConstructContainerForTests();

            var ProjectManager = container.Resolve<IProjectManager>();
            AssetManager = container.Resolve<IAssetManager>();
            HierarchyManager = container.Resolve<IHierarchyManager>();
            ScriptManager = container.Resolve<IScriptManager>();
            CommandFactory = container.Resolve<ICommandFactory>();
            TestRunner = container.Resolve<ITestRunner>();
            TestStatusManager = container.Resolve<ITestStatusManager>();
            TestFixtureManager = container.Resolve<ITestFixtureManager>();
            TestRunnerManager = container.Resolve<ITestRunnerManager>();

            ProjectManager.InitProjectNoScriptCompile(TempProjectPath);
        }

        [Test]
        public void CommandTypeFromPlugin_Changes_WhenPluginsAreReloaded()
        {
            AssetManager.CreateAsset(Properties.Resources.TestClassLibraryWithRefToRobot, k_UserDllPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var c1 = TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 14);

            AssetManager.RenameAsset(k_UserDllPath, k_UserDllNewPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var c2 = TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 14);

            Assert.AreNotEqual(c1.GetType(), c2.GetType(), "Types should be different");
            Assert.AreEqual(c1.Name, c2.Name, "Names though should be the same");
            Assert.AreEqual(c1.ToString(), c2.ToString(), "String representation should also be the same");
        }

        [Test]
        public void CommandsInHierarchy_GetReplacedWithNewType_AfterPluginReload()
        {
            AssetManager.CreateAsset(Properties.Resources.TestClassLibraryWithRefToRobot, k_UserDllPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var r = HierarchyManager.NewRecording();
            r.AddCommand(TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 14));
            r.AddCommand(TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 18));
            HierarchyManager.SaveRecording(r, k_RecordingPath);

            AssetManager.RenameAsset(k_UserDllPath, k_UserDllNewPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var c = r.First().value;
            var sample = TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 14);

            Assert.AreEqual(sample.GetType(), c.GetType(), "Types should be the same");
            Assert.AreEqual(sample.Name, c.Name, "Names though should be the same");
            Assert.AreEqual(sample.ToString(), c.ToString(), "String representation should also be the same");
            TestUtils.CheckThatGuidMapIsCorrect(r);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        // This is possible because BaseHierarchyManagerhas a method ReplaceCommandsInRecordingsWithNewRecompiledOnes
        public void CommandsInFixture_GetReplacedWithNewType_AfterPluginReload(bool isFixtureIsFromTestRunnerManager)
        {
            AssetManager.CreateAsset(Properties.Resources.TestClassLibraryWithRefToRobot, k_UserDllPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            // Making sure TestRunnerManager actually loads the test fixture
            AssetManager.CanLoadAssets = true;
            TestRunnerManager.ReloadTestFixtures();

            var f = TestFixtureManager.NewTestFixture(
                LightTestFixture.With(new Recording().With(
                    TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 14))));

            TestFixtureManager.SaveTestFixture(f, k_FixturePath);

            // Reload plugins
            AssetManager.RenameAsset(k_UserDllPath, k_UserDllNewPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            // Verify that recording is actually updated with new command type
            Recording rec = GetFirstRecording(isFixtureIsFromTestRunnerManager);
            var c = rec.First().value;
            var sample = TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 14);

            Assert.AreEqual(sample.GetType(), c.GetType(), "Types should be the same");
            Assert.AreEqual(sample.Name, c.Name, "Names though should be the same");
            Assert.AreEqual(sample.ToString(), c.ToString(), "String representation should also be the same");
            TestUtils.CheckThatGuidMapIsCorrect(rec);
        }

        private Recording GetFirstRecording(bool isFixtureIsFromTestRunnerManager)
        {
            if (isFixtureIsFromTestRunnerManager)
            {
                return TestRunnerManager.TestFixtures.First().Tests.First();
            }

            else
                return TestFixtureManager.Fixtures.First().Tests.First();
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
    }
}
