using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot;
using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Settings;
using System.Linq;
using Unity;

namespace Tests.Integration
{
    [TestClass]
    public class TypeCollectorIntegrationTests
    {
        private string TempProjectPath;

        IUnityContainer Container;
        IAssetManager AssetManager;
        IScriptTemplates ScriptTemplates;
        IScriptManager ScriptManager;

        private const string k_CustomCommandName = "Custom Command";
        private const string k_CustomCommandClassName = "CustomCommand";

        [TestInitialize]
        public void Initialize()
        {
            TempProjectPath = TestBase.GenerateProjectPath();
            Container = TestBase.ConstructContainerForTests();

            var ProjectManager = Container.Resolve<IProjectManager>();
            AssetManager = Container.Resolve<IAssetManager>();
            ScriptManager = Container.Resolve<IScriptManager>();
            ScriptTemplates = Container.Resolve<IScriptTemplates>();

            ProjectManager.InitProject(TempProjectPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            TestBase.TryCleanUp();
        }

        [TestMethod]
        public void CommandFactory_CollectsCommands_Correctly()
        {
            var CommandFactory = Container.Resolve<ICommandFactory>();
            var collector = Container.Resolve<ITypeCollector<Command>>();
            Assert.AreEqual(collector.AllTypes.Count(), CommandFactory.CommandNames.Count(), "Object count should be the same before compilation");

            TestBase.CopyAllTemplateScriptsToProjectFolder(ScriptTemplates, AssetManager);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            Assert.AreEqual(collector.AllTypes.Count(), CommandFactory.CommandNames.Count(), "Object count should be the same after compilation");
        }

        [TestMethod]
        public void SettingsManager_CollectsSettings_Correctly()
        {
            var SettingsManager = Container.Resolve<ISettingsManager>();
            var collector = Container.Resolve<ITypeCollector<BaseSettings>>();
            Assert.AreEqual(collector.AllTypes.Count(), SettingsManager.Settings.Count(), "Object count should be the same before compilation");

            TestBase.CopyAllTemplateScriptsToProjectFolder(ScriptTemplates, AssetManager);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            Assert.AreEqual(collector.AllTypes.Count(), SettingsManager.Settings.Count(), "Object count should be the same after compilation");
        }
    }
}
