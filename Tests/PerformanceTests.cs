using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Robot;
using Robot.Recordings;
using System.IO;
using Robot.Abstractions;
using RobotRuntime.Abstractions;
using Unity;
using RobotRuntime.Utils;
using Unity.Lifetime;
using RobotRuntime;
using RobotRuntime.Recordings;

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

        private const string k_RecordingAPath = "Recordings\\A.mrb";
        private const string k_RecordingBPath = "Recordings\\B.mrb";
        private const string k_RecordingCPath = "Recordings\\C.mrb";

        IMouseRobot MouseRobot;
        IAssetManager AssetManager;
        IAssetGuidManager AssetGuidManager;
        IProfiler Profiler;

        [TestMethod]
        public void AssetRefreshTime_100_RecordingAssets() // 22 ms - 50 (guid)
        {
            int count = 100;
            var name = "Refresh_Test_1";

            for (int i = 0; i < count; ++i)
                CreateDummyRecordingWithImporter("Recordings\\A" + i + ".mrb");

            Profiler.Start(name);
            AssetManager.Refresh();
            Profiler.Stop(name);

            Assert.AreEqual(count, AssetManager.Assets.Count(), "Asset count missmatch");

            var timeTaken = Profiler.CopyNodes()[name][0].Time;
            System.Diagnostics.Debug.WriteLine("Asset Refresh on 100 entries took: " + timeTaken + " ms.");

            Assert.IsTrue(timeTaken < 80, "This test took 40% longer than usual");
        }

        [TestMethod]
        public void AssetRefreshTime_1000_RecordingAssets() // 120 ms -- 180 (guid)
        {
            int count = 1000;
            var name = "Refresh_Test_2";

            for (int i = 0; i < count; ++i)
                CreateDummyRecordingWithImporter("Recordings\\A" + i + ".mrb");

            Profiler.Start(name);
            AssetManager.Refresh();
            Profiler.Stop(name);

            Assert.AreEqual(count, AssetManager.Assets.Count(), "Asset count missmatch");

            var timeTaken = Profiler.CopyNodes()[name][0].Time;
            System.Diagnostics.Debug.WriteLine("Asset Refresh on 1000 entries took: " + timeTaken + " ms.");

            Assert.IsTrue(timeTaken < 250, "This test took 40% longer than usual");
        }

        private static void CreateDummyRecordingWithImporter(string path)
        {
            var importer = EditorAssetImporter.FromPath(path);
            importer.Value = new Recording();
            importer.SaveAsset();
        }

        [TestInitialize]
        public void Initialize()
        {
            var container = new UnityContainer();
            RobotRuntime.Program.RegisterInterfaces(container);
            Robot.Program.RegisterInterfaces(container);

            container.RegisterType<ILogger, FakeLogger>(new ContainerControlledLifetimeManager());
            Logger.Instance = container.Resolve<ILogger>();

            MouseRobot = container.Resolve<IMouseRobot>();
            var ProjectManager = container.Resolve<IProjectManager>();
            AssetManager = container.Resolve<IAssetManager>();
            AssetGuidManager = container.Resolve<IAssetGuidManager>();
            Profiler = container.Resolve<IProfiler>();

            CleanupRecordingsDirectory();
            CleanupMetaDataDirectory();

            ProjectManager.InitProject(TempProjectPath);
            AssetManager.Refresh();
        }

        [TestCleanup]
        public void Cleanup()
        {
            CleanupRecordingsDirectory();
            CleanupMetaDataDirectory();

            AssetGuidManager.LoadMetaFiles();
            AssetManager.Refresh();
        }

        private void CleanupMetaDataDirectory()
        {
            if (Directory.Exists(Paths.MetadataPath))
            {
                DirectoryInfo di = new DirectoryInfo(Paths.MetadataPath);
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
            }
        }

        private void CleanupRecordingsDirectory()
        {
            if (Directory.Exists(TempProjectPath + "\\" + Paths.RecordingFolder))
            {
                DirectoryInfo di = new DirectoryInfo(TempProjectPath + "\\" + Paths.RecordingFolder);
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
            }
        }
    }
}
