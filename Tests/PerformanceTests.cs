using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Robot;
using Robot.Scripts;
using System.IO;
using RobotRuntime.Perf;

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

        [TestMethod]
        public void AssetRefreshTime_100_ScriptAssets() // 22 ms
        {
            int count = 100;

            for (int i = 0; i < count; ++i)
                CreateDummyScriptWithImporter("Scripts\\A" + i + ".mrb");

            Profiler.Start("Refresh_Test");
            AssetManager.Instance.Refresh();
            Profiler.Stop("Refresh_Test");

            Assert.AreEqual(count, AssetManager.Instance.Assets.Count(), "Asset count missmatch");

            var timeTaken = Profiler.Instace.CopyNodes()["Refresh_Test"][0].Time;
            System.Diagnostics.Debug.WriteLine("Asset Refresh on 100 entries took: " + timeTaken + " ms.");

            Assert.IsTrue(timeTaken < 31, "This test took 40% longer than usual");
        }

        [TestMethod]
        public void AssetRefreshTime_1000_ScriptAssets() // 120 ms
        {
            int count = 1000;

            for (int i = 0; i < count; ++i)
                CreateDummyScriptWithImporter("Scripts\\A" + i + ".mrb");

            Profiler.Start("Refresh_Test");
            AssetManager.Instance.Refresh();
            Profiler.Stop("Refresh_Test");

            Assert.AreEqual(count, AssetManager.Instance.Assets.Count(), "Asset count missmatch");

            var timeTaken = Profiler.Instace.CopyNodes()["Refresh_Test"][0].Time;
            System.Diagnostics.Debug.WriteLine("Asset Refresh on 1000 entries took: " + timeTaken + " ms.");

            Assert.IsTrue(timeTaken < 165, "This test took 40% longer than usual");
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
            MouseRobot.Instance.SetupProjectPath(TempProjectPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            DirectoryInfo di = new DirectoryInfo(TempProjectPath + "\\" + AssetManager.ScriptFolder);
            foreach (FileInfo file in di.GetFiles())
                file.Delete();

            AssetManager.Instance.Refresh();
        }
    }
}
