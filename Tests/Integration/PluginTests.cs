using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using Robot.Settings;
using RobotEditor.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Tests;
using System;
using System.IO;
using System.Linq;
using Unity;

namespace Tests.Integration
{
    [TestFixture]
    public class PluginTests : TestWithCleanup
    {
        private string TempProjectPath;

        IUnityContainer Container;

        IAssetManager AssetManager;
        IScriptTemplates ScriptTemplates;
        IScriptManager ScriptManager;
        ICommandFactory CommandFactory;
        ISolutionManager SolutionManager;
        ISettingsManager SettingsManager;
        IScriptLoader ScriptLoader;

        private const string k_UserDllName = "TestClassLibrary";
        private const string k_UserDllPath = "Assets\\" + k_UserDllName + ".dll";
        private const string k_SomeScript = "Assets\\SomeScript.cs";

        [SetUp]
        public void Initialize()
        {
            TempProjectPath = TestUtils.GenerateProjectPath();
            Container = TestUtils.ConstructContainerForTests();

            var ProjectManager = Container.Resolve<IProjectManager>();
            AssetManager = Container.Resolve<IAssetManager>();
            ScriptManager = Container.Resolve<IScriptManager>();
            ScriptTemplates = Container.Resolve<IScriptTemplates>();
            CommandFactory = Container.Resolve<ICommandFactory>();
            SolutionManager = Container.Resolve<ISolutionManager>();
            SettingsManager = Container.Resolve<ISettingsManager>();
            ScriptLoader = Container.Resolve<IScriptLoader>();

            ProjectManager.InitProject(TempProjectPath);
        }

        [Test]
        public void UserPlugins_AreLoaded_AndAccessible_UponAssetAddition()
        {
            AssetManager.CreateAsset(Properties.Resources.TestClassLibrary, k_UserDllPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var asm = ScriptLoader.IterateUserAssemblies(a => a).First(a =>
                a.ManifestModule.ScopeName.Equals(k_UserDllName + ".dll", StringComparison.InvariantCultureIgnoreCase));

            var type = asm.GetType("TestClassLibrary.Class");
            var c = Activator.CreateInstance(type);

            Assert.AreEqual("Class", c.GetType().Name, "Type names did not match");

            var ret = c.GetType().GetMethod("Method").Invoke(null, null);
            Assert.AreEqual(96, ret, "Return value from method from user plugin did not match");
        }

        [Test]
        public void Commands_FromPlugin_AppearInTypeCollector()
        {
            var collector = Container.Resolve<ITypeCollector<Command>>();
            var commandCountBefore = collector.AllTypes.Count();

            AssetManager.CreateAsset(Properties.Resources.TestClassLibraryWithRefToRobot, k_UserDllPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var commandCountAfter = collector.AllTypes.Count();

            Assert.AreEqual(commandCountAfter, commandCountBefore + 1, "Command from user plugin should appear in type collector");
        }

        [Test]
        public void Commands_FromPlugin_DisappearFromTypeCollector_AfterDeletion()
        {
            var collector = Container.Resolve<ITypeCollector<Command>>();
            var commandCountBefore = collector.AllTypes.Count();

            AssetManager.CreateAsset(Properties.Resources.TestClassLibraryWithRefToRobot, k_UserDllPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            AssetManager.DeleteAsset(k_UserDllPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var commandCountAfter = collector.AllTypes.Count();

            Assert.AreEqual(commandCountAfter, commandCountBefore, "Command count should stay the same");
        }

        [Test]
        public void Commands_FromPlugin_CanBeInstantiatedWith_CommandFactory()
        {
            AssetManager.CreateAsset(Properties.Resources.TestClassLibraryWithRefToRobot, k_UserDllPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var command = TestFixtureUtils.CreateCustomLogCommand(CommandFactory, 76);

            Assert.AreEqual("CommandLog", command.Name, "Command names differ");
            Assert.AreEqual("CommandLog", command.GetType().Name, "Command type name differ");
        }

        [Test]
        public void AddingPlugin_WillGenerateSolution_WithRefsToThosePlugins()
        {
            AssetManager.CreateAsset(Properties.Resources.TestClassLibrary, k_UserDllPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var proj = File.ReadAllText(SolutionManager.CSharpProjectPath);
            Assert.IsTrue(proj.Contains(k_UserDllPath), "Plugin Reference should be added to project file");
        }

        [Test]
        public void DeletingPlugin_WillGenerateSolution_WithoutRefsToThosePlugins()
        {
            AssetManager.CreateAsset(Properties.Resources.TestClassLibrary, k_UserDllPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            AssetManager.DeleteAsset(k_UserDllPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var proj = File.ReadAllText(SolutionManager.CSharpProjectPath);
            Assert.IsFalse(proj.Contains(k_UserDllPath), "Plugin Reference should be removed to project file");
        }

        [Test]
        public void AddingPlugin_WillAddReference_ToCompilerSettings()
        {
            AssetManager.CreateAsset(Properties.Resources.TestClassLibrary, k_UserDllPath);
            // No need to wait for compilation since settings are still on synchronous part

            var refs = SettingsManager.GetSettings<CompilerSettings>().CompilerReferencesFromProjectFolder;
            CollectionAssert.Contains(refs, k_UserDllPath, "Plugin Reference should be removed to project file");
        }

        [Test]
        public void DeletingPlugin_WillRemoveReference_FromCompilerSettings()
        {
            AssetManager.CreateAsset(Properties.Resources.TestClassLibrary, k_UserDllPath);
            AssetManager.DeleteAsset(k_UserDllPath);
            // No need to wait for compilation since settings are still on synchronous part

            var refs = SettingsManager.GetSettings<CompilerSettings>().CompilerReferencesFromProjectFolder;
            CollectionAssert.DoesNotContain(refs, k_UserDllPath, "Plugin Reference should be removed to project file");
        }

        [Test]
        public void DeletingPlugin_WillRemoveAssembly_FromScriptLoader()
        {
            AssetManager.CreateAsset(Properties.Resources.TestClassLibrary, k_UserDllPath);
            var res = ScriptManager.CompileScriptsAndReloadUserDomain().Result;

            var asmCountBefore = ScriptLoader.IterateUserAssemblies(a => a).Count();

            AssetManager.DeleteAsset(k_UserDllPath);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var asmCountAfter = ScriptLoader.IterateUserAssemblies(a => a).Count();

            Assert.AreEqual(asmCountBefore - 1, asmCountAfter, "Assembly count missmatch");
            Assert.AreEqual(1, asmCountAfter, "Only custom assembly should live");
        }

        [Test]
        public void UserScripts_CanReference_UserPlugins()
        {
            AssetManager.CreateAsset(Properties.Resources.TestClassLibrary, k_UserDllPath);
            AssetManager.CreateAsset(Properties.Resources.SomeClassUsingUserPlugin, k_SomeScript);
            var res = ScriptManager.CompileScriptsAndReloadUserDomain().Result;

            Assert.IsTrue(res, "Script compilation failed");
        }

        [Test]
        public void UserScripts_CanExecuteCode_FromUserPlugins()
        {
            AssetManager.CreateAsset(Properties.Resources.TestClassLibrary, k_UserDllPath);
            AssetManager.CreateAsset(Properties.Resources.SomeClassUsingUserPlugin, k_SomeScript);
            var res = ScriptManager.CompileScriptsAndReloadUserDomain().Result;

            Assert.IsTrue(res, "Script compilation failed");

            var type = ScriptLoader.IterateUserAssemblies(a => a).SelectMany(s => s.GetTypes())
                .First(t => t.Name.Equals("SomeClassUsingUserPlugin", StringComparison.InvariantCultureIgnoreCase));

            var c = Activator.CreateInstance(type);

            var ret = c.GetType().GetMethod("Method").Invoke(null, null);
            Assert.AreEqual(96, ret, "Return value from method from user plugin did not match");
        }
    }
}
