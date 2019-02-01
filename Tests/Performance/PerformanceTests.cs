using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot;
using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using System.Linq;
using Unity;

namespace Tests.Performance
{
    [TestClass]
    public class PerformanceTests
    {
        private string TempProjectPath;

        private const string k_RecordingAPath = "Assets\\A.mrb";
        private const string k_RecordingBPath = "Assets\\B.mrb";
        private const string k_RecordingCPath = "Assets\\C.mrb";

        IMouseRobot MouseRobot;
        IAssetManager AssetManager;
        IAssetGuidManager AssetGuidManager;
        IProfiler Profiler;

        [TestInitialize]
        public void Initialize()
        {
            TempProjectPath = TestBase.GenerateProjectPath();
            var container = TestBase.ConstructContainerForTests();

            MouseRobot = container.Resolve<IMouseRobot>();
            var ProjectManager = container.Resolve<IProjectManager>();
            AssetManager = container.Resolve<IAssetManager>();
            AssetGuidManager = container.Resolve<IAssetGuidManager>();
            Profiler = container.Resolve<IProfiler>();

            ProjectManager.InitProject(TempProjectPath);
            AssetManager.Refresh();
        }

        [TestMethod]
        public void AssetRefreshTime_100_RecordingAssets() // 22 ms - 50 (guid)
        {
            int count = 100;
            var name = "Refresh_Test_1";

            for (int i = 1; i < count; ++i)
                CreateDummyRecordingWithImporter("Assets\\A" + i + ".mrb");

            Profiler.Start(name);
            AssetManager.Refresh();
            Profiler.Stop(name);

            Assert.AreEqual(count, AssetManager.Assets.Count(), "Asset count missmatch");

            var timeTaken = Profiler.CopyNodes()[name][0].Time;
            System.Diagnostics.Debug.WriteLine("Asset Refresh on 100 entries took: " + timeTaken + " ms.");

            Assert.IsTrue(timeTaken < 150, "This test took 50% longer than usual");
        }

        [TestMethod]
        public void AssetRefreshTime_1000_RecordingAssets() // 120 ms -- 180 (guid)
        {
            int count = 1000;
            var name = "Refresh_Test_2";

            for (int i = 1; i < count; ++i)
                CreateDummyRecordingWithImporter("Assets\\A" + i + ".mrb");

            Profiler.Start(name);
            AssetManager.Refresh();
            Profiler.Stop(name);

            Assert.AreEqual(count, AssetManager.Assets.Count(), "Asset count missmatch");

            var timeTaken = Profiler.CopyNodes()[name][0].Time;
            System.Diagnostics.Debug.WriteLine("Asset Refresh on 1000 entries took: " + timeTaken + " ms.");

            Assert.IsTrue(timeTaken < 900, "This test took 40% longer than usual");
        }

        private static void CreateDummyRecordingWithImporter(string path)
        {
            var importer = EditorAssetImporter.FromPath(path);
            importer.Value = new Recording();
            importer.SaveAsset();
        }
    }
}
