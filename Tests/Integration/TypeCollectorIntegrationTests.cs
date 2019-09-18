using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using RobotRuntime.Settings;
using System.Linq;
using Unity;

namespace Tests.Integration
{
    [TestFixture]
    public class TypeCollectorIntegrationTests : TestWithCleanup
    {
        private string TempProjectPath;

        IUnityContainer Container;
        IAssetManager AssetManager;
        IScriptTemplates ScriptTemplates;
        IScriptManager ScriptManager;

        private const string k_CustomCommandName = "Custom Command";
        private const string k_CustomCommandClassName = "CustomCommand";

        [SetUp]
        public void Initialize()
        {
            TempProjectPath = TestUtils.GenerateProjectPath();
            Container = TestUtils.ConstructContainerForTests();

            var ProjectManager = Container.Resolve<IProjectManager>();
            AssetManager = Container.Resolve<IAssetManager>();
            ScriptManager = Container.Resolve<IScriptManager>();
            ScriptTemplates = Container.Resolve<IScriptTemplates>();

            TestUtils.InitProjectButDontWaitForScriptCompilation(TempProjectPath, Container);
        }

        [Test]
        public void CommandFactory_CollectsCommands_Correctly()
        {
            var CommandFactory = Container.Resolve<ICommandFactory>();
            var collector = Container.Resolve<ITypeCollector<Command>>();

            Assert.AreEqual(collector.AllTypes.Count(t => t != typeof(CommandUnknown)), CommandFactory.CommandNames.Count(), "Object count should be the same before compilation");

            TestUtils.CopyAllTemplateScriptsToProjectFolder(ScriptTemplates, AssetManager);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            Assert.AreEqual(collector.AllTypes.Count(t => t != typeof(CommandUnknown)), CommandFactory.CommandNames.Count(), "Object count should be the same after compilation");
        }

        [Test]
        public void SettingsManager_CollectsSettings_Correctly()
        {
            var SettingsManager = Container.Resolve<ISettingsManager>();
            var collector = Container.Resolve<ITypeCollector<BaseSettings>>();
            Assert.AreEqual(collector.AllTypes.Count(), SettingsManager.Settings.Count(), "Object count should be the same before compilation");

            TestUtils.CopyAllTemplateScriptsToProjectFolder(ScriptTemplates, AssetManager);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            Assert.AreEqual(collector.AllTypes.Count(), SettingsManager.Settings.Count(), "Object count should be the same after compilation");
        }
    }
}
