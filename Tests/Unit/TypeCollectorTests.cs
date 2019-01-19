using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot;
using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Commands;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Unity;

namespace Tests.Unit
{
    [TestClass]
    public class TypeCollectorTests
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

        [TestMethod]
        public void TypeCollector_JustAfterCreation_HasAllNativeTypes()
        {
            var collector = Container.Resolve<ITypeCollector<Command>>();
            var allCommandTypes = AppDomain.CurrentDomain.GetNativeAssemblies().GetAllTypesWhichImplementInterface(typeof(Command));

            Assert.AreEqual(allCommandTypes.Count(), collector.AllTypes.Count(), "Command type count should be the same");
            Assert.IsTrue(collector.AllTypes.Count() > 4, "Pretty arbitrary, just in case both these methods stop functioning");
        }

        [TestMethod]
        public void TypeObjectCollector_JustAfterCreation_HasAllNativeTypes()
        {
            var collector = Container.Resolve<ITypeObjectCollector<Command>>();
            var allCommandTypes = AppDomain.CurrentDomain.GetNativeAssemblies().GetAllTypesWhichImplementInterface(typeof(Command));

            Assert.AreEqual(allCommandTypes.Count(), collector.AllTypes.Count(), "Command type count should be the same");
            Assert.AreEqual(allCommandTypes.Count(), collector.AllObjects.Count(), "Command object count should be the same as type count");
            Assert.IsTrue(collector.AllTypes.Count() > 4, "Pretty arbitrary, just in case both these methods stop functioning");
        }

        [TestMethod]
        public void TypeObjectCollector_JustAfterCreation_HasCorrectNativeObjects()
        {
            var collector = Container.Resolve<ITypeObjectCollector<Command>>();
            var allCommandTypes = AppDomain.CurrentDomain.GetNativeAssemblies().GetAllTypesWhichImplementInterface(typeof(Command));

            Assert.AreEqual(collector.AllTypes.Count(), allCommandTypes.Count(), "Command type count should be the same");
            Assert.IsTrue(collector.AllTypes.Count() > 4, "Pretty arbitrary, just in case both these methods stop functioning");
        }

        [TestMethod]
        public void TypeObjectCollector_JustAfterCreation_HasNativeObjectsInstantiated()
        {
            var collector = Container.Resolve<ITypeObjectCollector<Command>>();
            var allCommandTypes = AppDomain.CurrentDomain.GetNativeAssemblies().GetAllTypesWhichImplementInterface(typeof(Command));

            var c1 = new CommandDown();
            var c2 = collector.AllObjects.FirstOrDefault(c => c.GetType() == typeof(CommandDown));
            Assert.AreEqual(c1.ToString(), c2.ToString(), "Commands should be the same");
        }

        [TestMethod]
        public void TypeCollector_JustAfterCreation_HasUserTypes()
        {
            TestBase.CopyAllTemplateScriptsToProjectFolder(ScriptTemplates, AssetManager);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var collector = Container.Resolve<ITypeCollector<Command>>();

            Assert.AreEqual(1, collector.UserTypes.Count(), "Only 1 user command should exist");
            Assert.AreEqual(k_CustomCommandClassName, collector.UserTypes.ElementAt(0).Name, "Command class name missmatch");
        }

        [TestMethod]
        public void TypeObjectCollector_JustAfterCreation_HasUserObjects()
        {
            TestBase.CopyAllTemplateScriptsToProjectFolder(ScriptTemplates, AssetManager);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var collector = Container.Resolve<ITypeObjectCollector<Command>>();

            Assert.AreEqual(1, collector.UserObjects.Count(), "Only 1 user command should exist");
            Assert.IsTrue(Regex.IsMatch(collector.UserObjects.ElementAt(0).ToString(), "Custom *Command", RegexOptions.IgnoreCase), "Command object representation missmatch");
        }

        [TestMethod]
        public void TypeCollector_AfterRecompilation_GetsNewUserTypes()
        {
            var collector = Container.Resolve<ITypeCollector<Command>>();
            var allObjects = collector.AllTypes.Count();

            TestBase.CopyAllTemplateScriptsToProjectFolder(ScriptTemplates, AssetManager);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            Assert.AreEqual(allObjects + 1, collector.AllTypes.Count(), "Type count should increase");
        }

        [TestMethod]
        public void TypeObjectCollector_AfterRecompilation_GetsNewUserObjects()
        {
            var collector = Container.Resolve<ITypeObjectCollector<Command>>();
            var allObjects = collector.AllObjects.Count();

            TestBase.CopyAllTemplateScriptsToProjectFolder(ScriptTemplates, AssetManager);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            Assert.AreEqual(allObjects + 1, collector.AllObjects.Count(), "Object count should increase");
        }
    }
}
