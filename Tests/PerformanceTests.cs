using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Robot;
using Robot.Scripts;
using System.IO;
using Robot.Abstractions;
using RobotRuntime.Abstractions;
using Unity;

namespace Tests
{
    [TestClass]
    public class PerformanceTests
    {
        private string TempProjectPath
        {
            get
            {
                return System.IO.Path.GetTempPath() + "\\MProject";
            }
        }

        private const string k_ScriptAPath = "Scripts\\A.mrb";
        private const string k_ScriptBPath = "Scripts\\B.mrb";
        private const string k_ScriptCPath = "Scripts\\C.mrb";

        IMouseRobot MouseRobot;
        IAssetManager AssetManager;
        IAssetGuidManager AssetGuidManager;
        IProfiler Profiler;

        [TestMethod]
        public void AssetRefreshTime_100_ScriptAssets() // 22 ms - 50 (guid)
        {
            int count = 100;
            var name = "Refresh_Test_1";

            for (int i = 0; i < count; ++i)
                CreateDummyScriptWithImporter("Scripts\\A" + i + ".mrb");

            Profiler.Start(name);
            AssetManager.Refresh();
            Profiler.Stop(name);

            Assert.AreEqual(count, AssetManager.Assets.Count(), "Asset count missmatch");

            var timeTaken = Profiler.CopyNodes()[name][0].Time;
            System.Diagnostics.Debug.WriteLine("Asset Refresh on 100 entries took: " + timeTaken + " ms.");

            Assert.IsTrue(timeTaken < 80, "This test took 40% longer than usual");
        }

        [TestMethod]
        public void AssetRefreshTime_1000_ScriptAssets() // 120 ms -- 180 (guid)
        {
            int count = 1000;
            var name = "Refresh_Test_2";

            for (int i = 0; i < count; ++i)
                CreateDummyScriptWithImporter("Scripts\\A" + i + ".mrb");

            Profiler.Start(name);
            AssetManager.Refresh();
            Profiler.Stop(name);

            Assert.AreEqual(count, AssetManager.Assets.Count(), "Asset count missmatch");

            var timeTaken = Profiler.CopyNodes()[name][0].Time;
            System.Diagnostics.Debug.WriteLine("Asset Refresh on 1000 entries took: " + timeTaken + " ms.");

            Assert.IsTrue(timeTaken < 250, "This test took 40% longer than usual");
        }

        private static void CreateDummyScriptWithImporter(string path)
        {
            var importer = EditorAssetImporter.FromPath(path);
            importer.Value = new Script();
            importer.SaveAsset();
        }

        [TestInitialize]
        public void Initialize()
        {
            var container = new UnityContainer();
            Robot.Program.RegisterInterfaces(container);
            RobotRuntime.Program.RegisterInterfaces(container);

            MouseRobot = container.Resolve<IMouseRobot>();
            AssetManager = container.Resolve<IAssetManager>();
            AssetGuidManager = container.Resolve<IAssetGuidManager>();
            Profiler = container.Resolve<IProfiler>();

            CleanupScriptsDirectory();
            CleanupMetaDataDirectory();

            MouseRobot.SetupProjectPath(TempProjectPath);
            AssetManager.Refresh();
        }

        [TestCleanup]
        public void Cleanup()
        {
            CleanupScriptsDirectory();
            CleanupMetaDataDirectory();

            AssetGuidManager.LoadMetaFiles();
            AssetManager.Refresh();
        }

        private void CleanupMetaDataDirectory()
        {
            if (Directory.Exists(AssetGuidManager.MetadataPath))
            {
                DirectoryInfo di = new DirectoryInfo(AssetGuidManager.MetadataPath);
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
            }
        }

        private void CleanupScriptsDirectory()
        {
            if (Directory.Exists(TempProjectPath + "\\" + AssetManager.ScriptFolder))
            {
                DirectoryInfo di = new DirectoryInfo(TempProjectPath + "\\" + AssetManager.ScriptFolder);
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
            }
        }
    }
}
