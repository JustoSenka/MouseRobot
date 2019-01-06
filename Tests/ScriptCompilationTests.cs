using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot;
using Robot.Abstractions;
using RobotEditor.Abstractions;
using RobotRuntime.Recordings;
using RobotRuntime.Utils;
using System;
using System.IO;
using Unity;

namespace Tests
{
    [TestClass]
    public class ScriptCompilationTests
    {
        private string TempProjectPath => Path.Combine(Path.GetTempPath(), "MProject");

        IMouseRobot MouseRobot;
        IAssetManager AssetManager;
        IHierarchyManager RecordingManager;
        IScriptTemplates ScriptTemplates;
        IScriptCompiler ScriptCompiler;
        IScriptManager ScriptManager;

        [TestMethod]
        public void ScriptTemplates_ShouldNotHave_CompilationErrors()
        {
            foreach (var name in ScriptTemplates.TemplateNames)
            {
                var script = ScriptTemplates.GetTemplate(name);
                var fileName = ScriptTemplates.GetTemplateFileName(name);
                var filePath = Path.Combine(Paths.ScriptPath, fileName + ".cs");
                filePath = Paths.GetUniquePath(filePath);
                AssetManager.CreateAsset(script, filePath);
            }

            Sync.WaitFor(() => !ScriptCompiler.IsCompiling);
            var compileSucceeded = ScriptManager.CompileScriptsAndReloadUserDomain().Result;

            Assert.IsTrue(compileSucceeded, "Script templates have compilation errors");
        }

        [TestInitialize]
        public void Initialize()
        {
            var container = TestBase.ConstructContainerForTests();

            MouseRobot = container.Resolve<IMouseRobot>();
            var ProjectManager = container.Resolve<IProjectManager>();
            AssetManager = container.Resolve<IAssetManager>();
            RecordingManager = container.Resolve<IHierarchyManager>();
            ScriptManager = container.Resolve<IScriptManager>();
            ScriptCompiler = container.Resolve<IScriptCompiler>();
            ScriptTemplates = container.Resolve<IScriptTemplates>();

            ProjectManager.InitProject(TempProjectPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            TestBase.TryCleanDirectory(TempProjectPath);
        }
    }
}
