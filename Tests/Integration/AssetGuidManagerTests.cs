using NUnit.Framework;
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

namespace Tests.Integration
{
    [TestFixture]
    public class AssetGuidManagerTests : TestWithCleanup
    {
        private string TempProjectPath;

        private const string k_RecordingAPath = "Assets\\A.mrb";
        private const string k_RecordingBPath = "Assets\\B.mrb";
        private const string k_RecordingCPath = "Assets\\C.mrb";

        private readonly static Guid guid = new Guid("12345678-9abc-def0-1234-567890123456");

        IAssetManager AssetManager;
        IAssetGuidManager AssetGuidManager;

        [SetUp]
        public void Initialize()
        {
            TempProjectPath = TestUtils.GenerateProjectPath();
            var container = TestUtils.ConstructContainerForTests(false);

            var ProjectManager = container.Resolve<IProjectManager>();
            AssetManager = container.Resolve<IAssetManager>();
            AssetGuidManager = container.Resolve<IAssetGuidManager>();

            ProjectManager.InitProject(TempProjectPath).Wait();
        }

        [Test]
        public void NewAssets_UponRefresh_AreAddedToGuidTable()
        {
            CreateDummyRecordingWithImporter(k_RecordingAPath);
            CreateDummyRecordingWithImporter(k_RecordingBPath);

            AssetManager.Refresh();

            Assert.AreEqual(3, AssetGuidManager.Paths.Count(), "Asset count missmatch");
            Assert.AreEqual(AssetManager.GetAsset(k_RecordingAPath).Guid, AssetGuidManager.GetGuid(k_RecordingAPath), "Asset guid missmatch");
            Assert.AreEqual(AssetManager.GetAsset(k_RecordingBPath).Guid, AssetGuidManager.GetGuid(k_RecordingBPath), "Asset guid missmatch");
        }

        [Test]
        public void DeletedAssets_UponRefresh_AreNotRemovedFromGuidTable()
        {
            CreateDummyRecordingWithImporter(k_RecordingAPath);
            CreateDummyRecordingWithImporter(k_RecordingBPath);
            CreateDummyRecordingWithImporter(k_RecordingCPath);
            AssetManager.Refresh();

            File.Delete(k_RecordingAPath);
            File.Delete(k_RecordingCPath);
            AssetManager.Refresh();

            Assert.AreEqual(2, AssetManager.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(4, AssetGuidManager.Paths.Count(), "Asset guid count missmatch");

            Assert.IsTrue(AssetGuidManager.ContainsValue(k_RecordingAPath), "Asset guid should still be in place");
            Assert.IsTrue(AssetGuidManager.ContainsValue(k_RecordingCPath), "Asset guid should still be in place");
            Assert.AreEqual(AssetManager.GetAsset(k_RecordingBPath).Guid, AssetGuidManager.GetGuid(k_RecordingBPath), "Asset guid missmatch");
        }

        [Test]
        public void RenamedAssets_UponRefresh_AreRenamedInGuidTable()
        {
            var assetA = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);
            var assetB = AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath);

            File.Move(k_RecordingBPath, k_RecordingCPath);
            AssetManager.Refresh();

            Assert.AreEqual(3, AssetGuidManager.Paths.Count(), "Asset count missmatch");
            Assert.AreEqual(default(Guid), AssetGuidManager.GetGuid(k_RecordingBPath), "B asset was renamed");

            Assert.AreEqual(assetA.Guid, AssetGuidManager.GetGuid(k_RecordingAPath), "A path gives correct guid");
            Assert.AreEqual(assetB.Guid, AssetGuidManager.GetGuid(k_RecordingCPath), "C path gives correct guid");

            Assert.AreEqual(k_RecordingAPath, AssetGuidManager.GetPath(assetA.Guid), "A still has same path");
            Assert.AreEqual(k_RecordingCPath, AssetGuidManager.GetPath(assetB.Guid), "B gives different path");
        }

        [Test]
        public void RenamedAssets_UponRefresh_AreGivenSameGuid_EvenIfTheyWereDeletedAndRestored()
        {
            var guidA = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath).Guid;
            var guidB = AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath).Guid;

            File.Move(k_RecordingBPath, "temp.mrb");
            CleanupRecordingsDirectory();
            AssetManager.Refresh();

            CreateDummyRecordingWithImporter(k_RecordingAPath);
            File.Move("temp.mrb", k_RecordingCPath);
            AssetManager.Refresh();

            Assert.AreEqual(3, AssetManager.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(3, AssetGuidManager.Paths.Count(), "Guid-Path map count missmatch");

            Assert.AreEqual(guidA, AssetManager.GetAsset(k_RecordingAPath).Guid, "A asset has correct guid");
            Assert.AreEqual(guidB, AssetManager.GetAsset(k_RecordingCPath).Guid, "C asset has correct guid");

            Assert.AreEqual(guidA, AssetGuidManager.GetGuid(k_RecordingAPath), "A path gives correct guid");
            Assert.AreEqual(guidB, AssetGuidManager.GetGuid(k_RecordingCPath), "C path gives correct guid");
        }

        [Test]
        public void Refresh_GivesAssetsSameGuid_IfTheyWereAlreadyKnown()
        {
            var guidA = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath).Guid;
            var guidB = AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath).Guid;

            File.Delete(k_RecordingBPath);
            AssetManager.Refresh();

            CreateDummyRecordingWithImporter(k_RecordingBPath);
            AssetManager.Refresh();

            Assert.AreEqual(3, AssetManager.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(3, AssetGuidManager.Paths.Count(), "Guid-Path map count missmatch");

            Assert.AreEqual(guidA, AssetManager.GetAsset(k_RecordingAPath).Guid, "A path gives correct guid");
            Assert.AreEqual(guidB, AssetManager.GetAsset(k_RecordingBPath).Guid, "B path gives correct guid");
        }

        [Test]
        public void DeletingAndRestoringAsset_WillGiveItSameGuid_AsItHadOriginally()
        {
            var guidA = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath).Guid;
            var guidB = AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath).Guid;

            AssetManager.DeleteAsset(k_RecordingBPath);

            CreateDummyRecordingWithImporter(k_RecordingBPath);
            AssetManager.Refresh();

            Assert.AreEqual(3, AssetManager.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(3, AssetGuidManager.Paths.Count(), "Guid-Path map count missmatch");

            Assert.AreEqual(guidA, AssetManager.GetAsset(k_RecordingAPath).Guid, "A path gives correct guid");
            Assert.AreEqual(guidB, AssetManager.GetAsset(k_RecordingBPath).Guid, "B path gives correct guid");
        }

        [Test]
        public void CreateAsset_AddsItToGuidTable()
        {
            var asset = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);
            var asset2 = AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath);

            Assert.AreEqual(3, AssetGuidManager.Paths.Count(), "Asset count missmatch");
            Assert.AreEqual(asset.Guid, AssetGuidManager.GetGuid(k_RecordingAPath), "Asset guid missmatch");
            Assert.AreEqual(asset2.Guid, AssetGuidManager.GetGuid(k_RecordingBPath), "Asset guid missmatch");
        }

        [Test]
        public void DeleteAsset_DoesNotRemoveIt_FromGuidTable()
        {
            var asset = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);
            var asset2 = AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath);

            AssetManager.DeleteAsset(k_RecordingAPath);

            Assert.AreEqual(2, AssetManager.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(3, AssetGuidManager.Paths.Count(), "Asset guid count missmatch");

            Assert.IsTrue(AssetGuidManager.ContainsValue(k_RecordingAPath), "Asset guid should still be in place");
            Assert.AreEqual(asset2.Guid, AssetGuidManager.GetGuid(k_RecordingBPath), "Asset guid missmatch");
        }

        [Test]
        public void RenameAsset_RenamesItInGuidTable()
        {
            var asset = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);
            var asset2 = AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath);

            AssetManager.RenameAsset(k_RecordingAPath, k_RecordingCPath);

            Assert.AreEqual(3, AssetGuidManager.Paths.Count(), "Asset count missmatch");
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

        private void CleanupRecordingsDirectory()
        {
            var path = Path.Combine(TempProjectPath, Paths.AssetsPath);
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
            }
        }
    }
}
