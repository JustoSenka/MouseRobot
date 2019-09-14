using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using Robot.Tests;
using RobotRuntime;
using RobotRuntime.Commands;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using System.Linq;
using System.Threading.Tasks;
using Unity;

namespace Tests.Integration
{
    [TestFixture]
    public class ProjectStartupTests : TestWithCleanup
    {
        private string TempProjectPath;

        ITestFixtureManager TestFixtureManager;
        IAssetManager AssetManager;
        IScriptManager ScriptManager;
        ICommandFactory CommandFactory;
        ITestRunnerManager TestRunnerManager;

        private const string k_FixturePath = "Assets\\" + k_FixtureName + ".mrt";
        private const string k_CustomCommandPath = "Assets\\CommandLog.cs";
        private const string k_TestName = "Test";
        private const string k_FixtureName = "Fixture";

        [SetUp]
        public void Initialize()
        {
            TempProjectPath = TestUtils.GenerateProjectPath();
            InitializeApplicationWithKnownProjectPath();
        }

        private Task InitializeApplicationWithKnownProjectPath()
        {
            var Container = TestUtils.ConstructContainerForTests();

            var ProjectManager = Container.Resolve<IProjectManager>();

            TestFixtureManager = Container.Resolve<ITestFixtureManager>();
            AssetManager = Container.Resolve<IAssetManager>();
            ScriptManager = Container.Resolve<IScriptManager>();
            CommandFactory = Container.Resolve<ICommandFactory>();
            TestRunnerManager = Container.Resolve<ITestRunnerManager>();

            return ProjectManager.InitProject(TempProjectPath);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ReloadingAssetManually_UpdatesUnknownCommands_AfterScriptsAreLoaded(bool waitForPluginLoad)
        {
            CreateOneFixtureWithCustomCommand();
            var initialization = InitializeApplicationWithKnownProjectPath();

            if (waitForPluginLoad)
                ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var fix = AssetManager.GetAsset(k_FixturePath).ReloadAsset<LightTestFixture>();
            var rec = fix.Tests.First(t => t.Name == k_TestName);
            var c = rec.Commands.First().value;

            AssertIfCommandIsCorrectType(waitForPluginLoad, c);
        }

        [Test]
        public void TestRunnerManager_UpdatesUnknownCommands_AfterScriptsAreLoaded()
        {
            CreateOneFixtureWithCustomCommand();
            var initialization = InitializeApplicationWithKnownProjectPath();

            // Force loading asset. It will load with unknown commands
            var fixture = AssetManager.GetAsset(k_FixturePath).Value;

            initialization.Wait();

            var fix = TestRunnerManager.TestFixtures.First(f => f.Name == k_FixtureName);
            var rec = fix.Tests.First(t => t.Name == k_TestName);
            var c = rec.Commands.First().value;

            AssertIfCommandIsCorrectType(true, c);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestRunnerManager_DoesNotLoadFixtures_BeforeFirstCompilationHappened(bool waitForPluginLoad)
        {
            CreateOneFixtureWithCustomCommand();
            var initialization = InitializeApplicationWithKnownProjectPath();

            if (waitForPluginLoad)
                initialization.Wait();

            var fixCount = TestRunnerManager.TestFixtures.Count;
            Assert.AreEqual(waitForPluginLoad ? 1 : 0, fixCount, "Loaded fixture count was incorrect");
        }


        [TestCase(true)]
        [TestCase(false)]
        public void TestFixtureManager_DoesNotUpdatesUnknownCommands_AfterScriptsAreLoaded(bool waitForPluginLoad)
        {
            CreateOneFixtureWithCustomCommand();
            var initialization = InitializeApplicationWithKnownProjectPath();

            var fix = AssetManager.GetAsset(k_FixturePath).ReloadAsset<LightTestFixture>();
            var fixture = TestFixtureManager.NewTestFixture(fix);

            if (waitForPluginLoad)
                initialization.Wait();

            var rec = fixture.Tests.First(t => t.Name == k_TestName);
            var c = rec.Commands.First().value;

            AssertIfCommandIsCorrectType(false, c); // TestFixtureManager should not update fixtures
            // user might have modifications, and reloading fixtures there will result in loss of work
            // UI will show a popup menu, so user has an option to reload fixture and discard changes or ignore
        }

        private static void AssertIfCommandIsCorrectType(bool waitForPluginLoad, Command c)
        {
            if (waitForPluginLoad)
                Assert.AreEqual("CommandLog", c.Name, "Unknown command should have been replaced with correct command after Assemblies are loaded. Did compilation/assembly load failed");
            else
                Assert.AreEqual(typeof(CommandUnknown), c.GetType(), "Types missmatched. Did the compilation and assembly load finished before assert method?");
        }

        private void CreateOneFixtureWithCustomCommand()
        {
            AssetManager.CreateAsset(Properties.Resources.CommandLog, k_CustomCommandPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            TestFixtureManager.SaveTestFixture(TestFixtureManager.NewTestFixture()
                .With(new Recording(k_TestName)
                .With(TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 1))), k_FixturePath);
        }
    }
}
