using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot;
using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Recordings;
using RobotRuntime.Utils;
using System;
using System.IO;
using System.Linq;
using Unity;
using Unity.Lifetime;

namespace Tests.Integration
{
    [TestClass]
    public class AssetGuidManagerTests
    {
        private string TempProjectPath;

        private const string k_RecordingAPath = "Recordings\\A.mrb";
        private const string k_RecordingBPath = "Recordings\\B.mrb";
        private const string k_RecordingCPath = "Recordings\\C.mrb";

        private readonly static Guid guid = new Guid("12345678-9abc-def0-1234-567890123456");

        IMouseRobot MouseRobot;
        IAssetManager AssetManager;
        IAssetGuidManager AssetGuidManager;

        [TestMethod]
        public void NewAssets_UponRefresh_AreAddedToGuidTable()
        {
            CreateDummyRecordingWithImporter(k_RecordingAPath);
            CreateDummyRecordingWithImporter(k_RecordingBPath);

            AssetManager.Refresh();

            Assert.AreEqual(2, AssetGuidManager.Paths.Count(), "Asset count missmatch");
            Assert.AreEqual(AssetManager.GetAsset(k_RecordingAPath).Guid, AssetGuidManager.GetGuid(k_RecordingAPath), "Asset guid missmatch");
            Assert.AreEqual(AssetManager.GetAsset(k_RecordingBPath).Guid, AssetGuidManager.GetGuid(k_RecordingBPath), "Asset guid missmatch");
        }

        [TestMethod]
        public void DeletedAssets_UponRefresh_AreNotRemovedFromGuidTable()
        {
            CreateDummyRecordingWithImporter(k_RecordingAPath);
            CreateDummyRecordingWithImporter(k_RecordingBPath);
            CreateDummyRecordingWithImporter(k_RecordingCPath);
            AssetManager.Refresh();

            File.Delete(k_RecordingAPath);
            File.Delete(k_RecordingCPath);
            AssetManager.Refresh();

            Assert.AreEqual(1, AssetManager.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(3, AssetGuidManager.Paths.Count(), "Asset guid count missmatch");

            Assert.IsTrue(AssetGuidManager.ContainsValue(k_RecordingAPath), "Asset guid should still be in place");
            Assert.IsTrue(AssetGuidManager.ContainsValue(k_RecordingCPath), "Asset guid should still be in place");
            Assert.AreEqual(AssetManager.GetAsset(k_RecordingBPath).Guid, AssetGuidManager.GetGuid(k_RecordingBPath), "Asset guid missmatch");
        }

        [TestMethod]
        public void RenamedAssets_UponRefresh_AreRenamedInGuidTable()
        {
            var assetA = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);
            var assetB = AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath);

            File.Move(k_RecordingBPath, k_RecordingCPath);
            AssetManager.Refresh();

            Assert.AreEqual(2, AssetGuidManager.Paths.Count(), "Asset count missmatch");
            Assert.AreEqual(default(Guid), AssetGuidManager.GetGuid(k_RecordingBPath), "B asset was renamed");

            Assert.AreEqual(assetA.Guid, AssetGuidManager.GetGuid(k_RecordingAPath), "A path gives correct guid");
            Assert.AreEqual(assetB.Guid, AssetGuidManager.GetGuid(k_RecordingCPath), "C path gives correct guid");

            Assert.AreEqual(k_RecordingAPath, AssetGuidManager.GetPath(assetA.Guid), "A still has same path");
            Assert.AreEqual(k_RecordingCPath, AssetGuidManager.GetPath(assetB.Guid), "B gives different path");
        }

        [TestMethod]
        public void RenamedAssets_UponRefresh_AreGivenSameGuid()
        {
            var guidA = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath).Guid;
            var guidB = AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath).Guid;

            CleanupRecordingsDirectory();
            AssetManager.Refresh();

            CreateDummyRecordingWithImporter(k_RecordingAPath);
            CreateDummyRecordingWithImporter(k_RecordingCPath);
            AssetManager.Refresh();

            Assert.AreEqual(2, AssetManager.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(2, AssetGuidManager.Paths.Count(), "Guid-Path map count missmatch");


            Assert.AreEqual(guidA, AssetManager.GetAsset(k_RecordingAPath).Guid, "A asset has correct guid");
            Assert.AreEqual(guidB, AssetManager.GetAsset(k_RecordingCPath).Guid, "C asset has correct guid");

            Assert.AreEqual(guidA, AssetGuidManager.GetGuid(k_RecordingAPath), "A path gives correct guid");
            Assert.AreEqual(guidB, AssetGuidManager.GetGuid(k_RecordingCPath), "C path gives correct guid");
        }

        [TestMethod]
        public void Refresh_GivesAssetsSameGuid_IfTheyWereAlreadyKnown()
        {
            var guidA = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath).Guid;
            var guidB = AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath).Guid;

            File.Delete(k_RecordingBPath);
            AssetManager.Refresh();

            CreateDummyRecordingWithImporter(k_RecordingBPath);
            AssetManager.Refresh();

            Assert.AreEqual(2, AssetManager.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(2, AssetGuidManager.Paths.Count(), "Guid-Path map count missmatch");

            Assert.AreEqual(guidA, AssetManager.GetAsset(k_RecordingAPath).Guid, "A path gives correct guid");
            Assert.AreEqual(guidB, AssetManager.GetAsset(k_RecordingBPath).Guid, "B path gives correct guid");
        }

        [TestMethod]
        public void DeletingAndRestoringAsset_WillGiveItSameGuid_AsItHadOriginally()
        {
            var guidA = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath).Guid;
            var guidB = AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath).Guid;

            AssetManager.DeleteAsset(k_RecordingBPath);

            CreateDummyRecordingWithImporter(k_RecordingBPath);
            AssetManager.Refresh();

            Assert.AreEqual(2, AssetManager.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(2, AssetGuidManager.Paths.Count(), "Guid-Path map count missmatch");

            Assert.AreEqual(guidA, AssetManager.GetAsset(k_RecordingAPath).Guid, "A path gives correct guid");
            Assert.AreEqual(guidB, AssetManager.GetAsset(k_RecordingBPath).Guid, "B path gives correct guid");
        }

        [TestMethod]
        public void CreateAsset_AddsItToGuidTable()
        {
            var asset = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);
            var asset2 = AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath);

            Assert.AreEqual(2, AssetGuidManager.Paths.Count(), "Asset count missmatch");
            Assert.AreEqual(asset.Guid, AssetGuidManager.GetGuid(k_RecordingAPath), "Asset guid missmatch");
            Assert.AreEqual(asset2.Guid, AssetGuidManager.GetGuid(k_RecordingBPath), "Asset guid missmatch");
        }

        [TestMethod]
        public void DeleteAsset_DoesNotRemoveIt_FromGuidTable()
        {
            var asset = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);
            var asset2 = AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath);

            AssetManager.DeleteAsset(k_RecordingAPath);

            Assert.AreEqual(1, AssetManager.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(2, AssetGuidManager.Paths.Count(), "Asset guid count missmatch");

            Assert.IsTrue(AssetGuidManager.ContainsValue(k_RecordingAPath), "Asset guid should still be in place");
            Assert.AreEqual(asset2.Guid, AssetGuidManager.GetGuid(k_RecordingBPath), "Asset guid missmatch");
        }

        [TestMethod]
        public void RenameAsset_RenamesItInGuidTable()
        {
            var asset = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);
            var asset2 = AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath);

            AssetManager.RenameAsset(k_RecordingAPath, k_RecordingCPath);

            Assert.AreEqual(2, AssetGuidManager.Paths.Count(), "Asset count missmatch");
            Assert.IsFalse(AssetGuidManager.ContainsValue(k_RecordingAPath), "A path should be deleted");

            Assert.AreEqual(default(Guid), AssetGuidManager.GetGuid(k_RecordingAPath), "Asset guid missmatch");
            Assert.AreEqual(asset2.Guid, AssetGuidManager.GetGuid(k_RecordingBPath), "Asset guid missmatch");
            Assert.AreEqual(asset.Guid, AssetGuidManager.GetGuid(k_RecordingCPath), "Asset guid missmatch");
        }

        private static void CreateDummyRecordingWithImporter(string path)
        {
            var importer = EditorAssetImporter.FromPath(path);
            importer.Value = new Recording(guid);
            importer.SaveAsset();
        }

        [TestInitialize]
        public void Initialize()
        {
            TempProjectPath = TestBase.GenerateProjectPath();

            var container = new UnityContainer();
            RobotRuntime.Program.RegisterInterfaces(container);
            Robot.Program.RegisterInterfaces(container);
            container.RegisterType<ILogger, FakeLogger>(new ContainerControlledLifetimeManager());
            Logger.Instance = container.Resolve<ILogger>();

            MouseRobot = container.Resolve<IMouseRobot>();
            var ProjectManager = container.Resolve<IProjectManager>();
            AssetManager = container.Resolve<IAssetManager>();
            AssetGuidManager = container.Resolve<IAssetGuidManager>();

            ProjectManager.InitProject(TempProjectPath);

            CleanupRecordingsDirectory();
            CleanupMetaDataDirectory();

            AssetGuidManager.LoadMetaFiles();
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
