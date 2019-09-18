using NUnit.Framework;
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
    [TestFixture]
    public class TypeCollectorTests : TestWithCleanup
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

            ProjectManager.InitProjectNoScriptCompile(TempProjectPath);
        }

        [Test]
        public void TypeCollector_JustAfterCreation_HasAllNativeTypes()
        {
            var collector = Container.Resolve<ITypeCollector<Command>>();
            var allCommandTypes = AppDomain.CurrentDomain.GetNativeAssemblies().GetAllTypesWhichImplementInterface(typeof(Command));

            Assert.AreEqual(allCommandTypes.Count(), collector.AllTypes.Count(), "Command type count should be the same");
            Assert.IsTrue(collector.AllTypes.Count() > 4, "Pretty arbitrary, just in case both these methods stop functioning");
        }

        [Test]
        public void TypeObjectCollector_JustAfterCreation_HasAllNativeTypes()
        {
            var collector = Container.Resolve<ITypeObjectCollector<Command>>();
            var allCommandTypes = AppDomain.CurrentDomain.GetNativeAssemblies().GetAllTypesWhichImplementInterface(typeof(Command));

            Assert.AreEqual(allCommandTypes.Count(), collector.AllTypes.Count(), "Command type count should be the same");
            Assert.AreEqual(allCommandTypes.Count(), collector.AllObjects.Count(), "Command object count should be the same as type count");
            Assert.IsTrue(collector.AllTypes.Count() > 4, "Pretty arbitrary, just in case both these methods stop functioning");
        }

        [Test]
        public void TypeObjectCollector_JustAfterCreation_HasCorrectNativeObjects()
        {
            var collector = Container.Resolve<ITypeObjectCollector<Command>>();
            var allCommandTypes = AppDomain.CurrentDomain.GetNativeAssemblies().GetAllTypesWhichImplementInterface(typeof(Command));

            Assert.AreEqual(collector.AllTypes.Count(), allCommandTypes.Count(), "Command type count should be the same");
            Assert.IsTrue(collector.AllTypes.Count() > 4, "Pretty arbitrary, just in case both these methods stop functioning");
        }

        [Test]
        public void TypeObjectCollector_JustAfterCreation_HasNativeObjectsInstantiated()
        {
            var collector = Container.Resolve<ITypeObjectCollector<Command>>();
            var allCommandTypes = AppDomain.CurrentDomain.GetNativeAssemblies().GetAllTypesWhichImplementInterface(typeof(Command));

            var c1 = new CommandDown();
            var c2 = collector.AllObjects.FirstOrDefault(c => c.GetType() == typeof(CommandDown));
            Assert.AreEqual(c1.ToString(), c2.ToString(), "Commands should be the same");
        }

        [Test]
        public void TypeCollector_JustAfterCreation_HasUserTypes()
        {
            TestUtils.CopyAllTemplateScriptsToProjectFolder(ScriptTemplates, AssetManager);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var collector = Container.Resolve<ITypeCollector<Command>>();

            Assert.AreEqual(1, collector.UserTypes.Count(), "Only 1 user command should exist");
            Assert.AreEqual(k_CustomCommandClassName, collector.UserTypes.ElementAt(0).Name, "Command class name missmatch");
        }

        [Test]
        public void TypeObjectCollector_JustAfterCreation_HasUserObjects()
        {
            TestUtils.CopyAllTemplateScriptsToProjectFolder(ScriptTemplates, AssetManager);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            var collector = Container.Resolve<ITypeObjectCollector<Command>>();

            Assert.AreEqual(1, collector.UserObjects.Count(), "Only 1 user command should exist");
            Assert.IsTrue(Regex.IsMatch(collector.UserObjects.ElementAt(0).ToString(), "Custom *Command", RegexOptions.IgnoreCase), "Command object representation missmatch");
        }

        [Test]
        public void TypeCollector_AfterRecompilation_GetsNewUserTypes()
        {
            var collector = Container.Resolve<ITypeCollector<Command>>();
            var allObjects = collector.AllTypes.Count();

            TestUtils.CopyAllTemplateScriptsToProjectFolder(ScriptTemplates, AssetManager);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            Assert.AreEqual(allObjects + 1, collector.AllTypes.Count(), "Type count should increase");
        }

        [Test]
        public void TypeCollector_GetsCommand_AfterRecompilation()
        {
            var collector = Container.Resolve<ITypeCollector<Command>>();
            AssetManager.CreateAsset(Properties.Resources.CommandLog, "Assets\\path.cs");
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

        }


        [Test]
        public void TypeObjectCollector_AfterRecompilation_GetsNewUserObjects()
        {
            var collector = Container.Resolve<ITypeObjectCollector<Command>>();
            var allObjects = collector.AllObjects.Count();

            TestUtils.CopyAllTemplateScriptsToProjectFolder(ScriptTemplates, AssetManager);
            ScriptManager.CompileScriptsAndReloadUserDomain().Wait();

            Assert.AreEqual(allObjects + 1, collector.AllObjects.Count(), "Object count should increase");
        }
    }
}
