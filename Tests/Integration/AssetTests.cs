using Microsoft.VisualStudio.TestTools.UnitTesting;
using Robot;
using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Commands;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using System;
using System.IO;
using System.Linq;
using Unity;

namespace Tests.Integration
{
    [TestClass]
    public class AssetTests
    {
        private string TempProjectPath;

        private const string k_RecordingAPath = "Assets\\A.mrb";
        private const string k_RecordingBPath = "Assets\\B.mrb";
        private const string k_RecordingCPath = "Assets\\C.mrb";
        private const string k_RecordingDPath = "Assets\\D.mrb";

        private const string k_FixtureAPath = "Assets\\A.mrt";
        private const string k_RecordingANestedPath = "Assets\\folder\\A.mrb";

        private const string k_FolderA = "Assets\\folderA\\";
        private const string k_FolderB = "Assets\\folderB\\";
        private const string k_FolderANested = "Assets\\folderA\\nestedFolder";

        private const string k_RecInFolderA = k_FolderA + "rec.mrb";
        private const string k_FixInFolderA = k_FolderA + "fix.mrt";

        private const string k_RecInFolderB = k_FolderB + "rec.mrb";
        private const string k_FixInFolderB = k_FolderB + "fix.mrt";

        private readonly static Guid guid = new Guid("12345678-9abc-def0-1234-567890123456");

        IAssetManager AssetManager;
        IHierarchyManager RecordingManager;
        ITestFixtureManager TestFixtureManager;

        [TestInitialize]
        public void Initialize()
        {
            TempProjectPath = TestBase.GenerateProjectPath();
            var container = TestBase.ConstructContainerForTests(false);

            var ProjectManager = container.Resolve<IProjectManager>();
            AssetManager = container.Resolve<IAssetManager>();
            RecordingManager = container.Resolve<IHierarchyManager>();
            TestFixtureManager = container.Resolve<ITestFixtureManager>();

            ProjectManager.InitProject(TempProjectPath);
        }
        [TestMethod]
        public void TwoIdenticalAssets_HaveTheSameHash_ButDifferentGuids()
        {
            var asset = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);

            Directory.CreateDirectory(new FileInfo(k_RecordingANestedPath).Directory.FullName);
            var asset2 = AssetManager.CreateAsset(new Recording(guid), k_RecordingANestedPath);

            Assert.AreEqual(asset.Hash, asset2.Hash, "Identical assets should have same hash");
            Assert.AreNotEqual(asset.Guid, asset2.Guid, "Identical assets should have different GUIDs");
        }

        [TestMethod]
        public void CreateAsset_CreatesFile_AndAddsToAssetManager()
        {
            var recording = new Recording(guid);
            AssetManager.CreateAsset(recording, k_RecordingAPath);

            Assert.IsTrue(File.Exists(k_RecordingAPath), "File was not created");
            Assert.AreEqual(AssetManager.GetAsset(k_RecordingAPath).Importer.Load<Recording>(), recording, "Asset value did not match the recording");
        }

        [TestMethod]
        public void ReloadRecording_UpdatesRecordingRef_FromFile()
        {
            var recording = RecordingManager.NewRecording();
            RecordingManager.SaveRecording(recording, k_RecordingAPath);
            recording.AddCommand(new CommandSleep(5));

            var recording2 = RecordingManager.LoadRecording(k_RecordingAPath);

            Assert.AreNotEqual(recording, recording2, "Reloading recording should give new reference type object");
            Assert.AreEqual(recording2, RecordingManager.LoadedRecordings[0], "Recordings from assets and in recording manager should be the same");
            Assert.AreEqual(0, recording2.Commands.Count(), "Recording should not have commands");
        }

        [TestMethod]
        public void RenameAsset_RenamesFile_AndKeepsAllReferencesIntact()
        {
            var recording = new Recording(guid);
            var asset = AssetManager.CreateAsset(recording, k_RecordingAPath);

            AssetManager.RenameAsset(k_RecordingAPath, k_RecordingBPath);

            var newAsset = AssetManager.GetAsset(k_RecordingBPath);
            var newRecording = newAsset.Importer.Load<Recording>();

            Assert.AreEqual(asset, newAsset, "Asset reference should be the same after renaming asset");
            Assert.AreEqual(recording, newRecording, "Recording reference should be the same after renaming asset");
            Assert.IsFalse(File.Exists(k_RecordingAPath), "Old file should not exist anymore");
            Assert.IsTrue(File.Exists(k_RecordingBPath), "New file should exist on disk");
        }

        [TestMethod]
        public void Refresh_WithFileBeingRenamedOnFileSystem_AcceptsRenameAndKeepsAllReferences()
        {
            var recording = new Recording(guid);
            var asset = AssetManager.CreateAsset(recording, k_RecordingAPath);

            File.Move(k_RecordingAPath, k_RecordingBPath);
            AssetManager.Refresh();

            var newAsset = AssetManager.GetAsset(k_RecordingBPath);
            var newRecording = newAsset.Importer.Load<Recording>();

            Assert.AreEqual(asset, newAsset, "Asset reference should be the same after renaming asset");
            Assert.AreEqual(recording, newRecording, "Recording reference should be the same after renaming asset");
            Assert.IsFalse(File.Exists(k_RecordingAPath), "Old file should not exist anymore");
            Assert.IsTrue(File.Exists(k_RecordingBPath), "New file should exist on disk");
        }

        [TestMethod]
        public void Refresh_WithDeletedFiles_RemovesThemFromManager()
        {
            AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);
            AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath);
            AssetManager.CreateAsset(new Recording(guid), k_RecordingCPath);

            File.Delete(k_RecordingAPath);
            File.Delete(k_RecordingCPath);
            AssetManager.Refresh();

            var assetA = AssetManager.GetAsset(k_RecordingAPath);
            var assetB = AssetManager.GetAsset(k_RecordingBPath);
            var assetC = AssetManager.GetAsset(k_RecordingCPath);

            Assert.AreEqual(2, AssetManager.Assets.Count());
            Assert.IsNull(assetA, "Asset A should not exist anymore");
            Assert.IsNotNull(assetB, "Asset B should exist");
            Assert.IsNull(assetC, "Asset C should not exist anymore");
        }

        [TestMethod]
        public void Refresh_WithNewFiles_AddsThemToManager()
        {
            AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);

            CreateDummyRecordingWithImporter(k_RecordingBPath);
            CreateDummyRecordingWithImporter(k_RecordingCPath);
            AssetManager.Refresh();

            var assetA = AssetManager.GetAsset(k_RecordingAPath);
            var assetB = AssetManager.GetAsset(k_RecordingBPath);
            var assetC = AssetManager.GetAsset(k_RecordingCPath);

            Assert.AreEqual(4, AssetManager.Assets.Count());
            Assert.IsNotNull(assetA, "Asset A should exist");
            Assert.IsNotNull(assetB, "Asset B should exist");
            Assert.IsNotNull(assetC, "Asset C should exist");
        }

        [TestMethod]
        public void Refresh_WithRenamedFiles_HavingOtherFilesWithSameHash_AcceptsRename()
        {
            var assetA = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);
            var assetB = AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath);
            var assetC = AssetManager.CreateAsset(new Recording(guid), k_RecordingCPath);

            File.Move(k_RecordingBPath, k_RecordingDPath);
            AssetManager.Refresh();

            Assert.AreEqual(4, AssetManager.Assets.Count());
            Assert.AreEqual(assetA.Guid, AssetManager.GetAsset(k_RecordingAPath).Guid, "A asset guid missmath");
            Assert.AreEqual(assetB.Guid, AssetManager.GetAsset(k_RecordingDPath).Guid, "B asset guid missmath");
            Assert.AreEqual(assetC.Guid, AssetManager.GetAsset(k_RecordingCPath).Guid, "C asset guid missmath");
            Assert.IsNull(AssetManager.GetAsset(k_RecordingBPath), "B asset was renamed, so should not appear");
        }

        [TestMethod]
        public void Refresh_WithRenamedFiles_AndOneBeingDeleted_AcceptsRename()
        {
            var assetA = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);
            var assetB = AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath);
            var assetC = AssetManager.CreateAsset(new Recording(guid), k_RecordingCPath);

            File.Move(k_RecordingBPath, k_RecordingDPath);
            File.Delete(k_RecordingAPath);
            AssetManager.Refresh();

            Assert.AreEqual(3, AssetManager.Assets.Count());
            Assert.AreEqual(assetB.Guid, AssetManager.GetAsset(k_RecordingDPath).Guid, "B asset guid missmath");
            Assert.AreEqual(assetC.Guid, AssetManager.GetAsset(k_RecordingCPath).Guid, "C asset guid missmath");
            Assert.IsNull(AssetManager.GetAsset(k_RecordingAPath), "A asset was deleted, so should not appear");
            Assert.IsNull(AssetManager.GetAsset(k_RecordingBPath), "B asset was renamed, so should not appear");
        }

        [TestMethod]
        public void DeleteAsset_RemovesFromManager_AndFromDisk()
        {
            AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);
            AssetManager.CreateAsset(new Recording(guid), k_RecordingBPath);

            AssetManager.DeleteAsset(k_RecordingAPath);

            Assert.AreEqual(2, AssetManager.Assets.Count());
            Assert.IsFalse(File.Exists(k_RecordingAPath), "Asset A should not exist on disk");
            Assert.IsNull(AssetManager.GetAsset(k_RecordingAPath), "Asset A should not exist anymore");
            Assert.IsNotNull(AssetManager.GetAsset(k_RecordingBPath), "Asset B should exist");
        }

        [TestMethod]
        public void DeleteDirectory_RemovesFromManager_AndFromDisk()
        {
            var path = "Assets\\someFolder";
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "\\Assets");
            AssetManager.Refresh();

            AssetManager.CreateAsset(new Recording(guid), Path.Combine(path, k_RecordingAPath));
            AssetManager.CreateAsset(new Recording(guid), Path.Combine(path, k_RecordingBPath));

            Assert.AreEqual(5, AssetManager.Assets.Count(), "Not all assets were collected in the first place"); // Dirs included

            AssetManager.DeleteAsset(path);

            Assert.AreEqual(1, AssetManager.Assets.Count(), "After deletion, only one asset should be left");
        }

        [TestMethod]
        public void CreateFolderAsset_AddsAssetToManager_AndCreatesDirAtDisk()
        {
            var path = "Assets\\someFolder";
            AssetManager.CreateAsset(null, path);

            Assert.AreEqual(2, AssetManager.Assets.Count(), "Two dir assets should be known");
            Assert.AreEqual(path.NormalizePath(), AssetManager.GetAsset(path).Importer.Load<string>().NormalizePath());
        }

        [TestMethod]
        public void CreateAsset_WhenFolderDoesNotExist_ReturnsNull()
        {
            var path = "Assets\\non existant folder\\rec.mrb";
            var res = AssetManager.CreateAsset(new Recording(), path);

            Assert.IsNull(res, "Cannot create assets when folder does not exist");
        }

        [TestMethod]
        public void CreateFolderAsset_WhenFolderDoesNotExist_ReturnsNull()
        {
            var path = "Assets\\non existant folder\\someFolder";
            var res = AssetManager.CreateAsset(null, path);

            Assert.IsNull(res, "Cannot create assets when folder does not exist");
        }

        [TestMethod]
        public void AssetManager_CanAddTwoAssets_WithSameName_ButDifferentExtension()
        {
            AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);
            AssetManager.CreateAsset(TestFixtureManager.NewTestFixture().ToLightTestFixture(), k_FixtureAPath);

            AssetManager.Refresh();

            Assert.AreEqual(3, AssetManager.Assets.Count(), "Two assets + directory should be found");
        }

        [TestMethod]
        public void AssetManager_CanRefreshTwoAssets_WithSameName_ButDifferentExtension()
        {
            CreateDummyRecordingWithImporter(k_RecordingAPath);
            AssetManager.CreateAsset(TestFixtureManager.NewTestFixture().ToLightTestFixture(), k_FixtureAPath);

            Assert.AreEqual(2, AssetManager.Assets.Count(), "One assets + directory should be found");
            AssetManager.Refresh();

            Assert.AreEqual(3, AssetManager.Assets.Count(), "Two assets + directory should be found");
        }

        [TestMethod]
        public void AssetManager_CanReturnTwoAssets_WithSameName_ButDifferentExtension()
        {
            AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);
            AssetManager.CreateAsset(TestFixtureManager.NewTestFixture().ToLightTestFixture(), k_FixtureAPath);

            Assert.AreEqual(typeof(Recording), AssetManager.GetAsset(k_RecordingAPath).Importer.HoldsType());
            Assert.AreEqual(typeof(LightTestFixture), AssetManager.GetAsset(k_FixtureAPath).Importer.HoldsType());
        }

        [TestMethod]
        public void RenameFolder_WithAssetsInside_WillKeepAllGuids()
        {
            Directory.CreateDirectory(k_FolderA);

            var rec = AssetManager.CreateAsset(new Recording(), k_RecInFolderA);
            var fix = AssetManager.CreateAsset(TestFixtureManager.NewTestFixture().ToLightTestFixture(), k_FixInFolderA);

            Directory.Move(k_FolderA, k_FolderB);
            AssetManager.Refresh();

            Assert.AreEqual(4, AssetManager.Assets.Count(), "Asset count missmatch");

            Assert.AreEqual(typeof(Recording), AssetManager.GetAsset(k_RecInFolderB).Importer.HoldsType());
            Assert.AreEqual(typeof(LightTestFixture), AssetManager.GetAsset(k_FixInFolderB).Importer.HoldsType());

            Assert.AreEqual(rec.Guid, AssetManager.GetAsset(k_RecInFolderB).Guid);
            Assert.AreEqual(fix.Guid, AssetManager.GetAsset(k_FixInFolderB).Guid);
        }

        [TestMethod]
        public void RenameFolder_FromAssetManager_WithAssetsInside_WillKeepAllGuids()
        {
            AssetManager.CreateAsset(null, k_FolderA);

            var rec = AssetManager.CreateAsset(new Recording(), k_RecInFolderA);
            var fix = AssetManager.CreateAsset(TestFixtureManager.NewTestFixture().ToLightTestFixture(), k_FixInFolderA);

            AssetManager.RenameAsset(k_FolderA, k_FolderB);

            Assert.AreEqual(4, AssetManager.Assets.Count(), "Asset count missmatch");

            Assert.AreEqual(typeof(Recording), AssetManager.GetAsset(k_RecInFolderB).Importer.HoldsType());
            Assert.AreEqual(typeof(LightTestFixture), AssetManager.GetAsset(k_FixInFolderB).Importer.HoldsType());

            Assert.AreEqual(rec.Guid, AssetManager.GetAsset(k_RecInFolderB).Guid);
            Assert.AreEqual(fix.Guid, AssetManager.GetAsset(k_FixInFolderB).Guid);
        }

        [TestMethod]
        public void RenameFolder_WithAssetsInside_WillFireOnlyOneCallback()
        {
            AssetManager.CreateAsset(null, k_FolderA);

            var rec = AssetManager.CreateAsset(new Recording(), k_RecInFolderA);
            var fix = AssetManager.CreateAsset(TestFixtureManager.NewTestFixture().ToLightTestFixture(), k_FixInFolderA);

            var callbackitCount = 0;
            AssetManager.AssetRenamed += (from, to) =>
            {
                callbackitCount++;
                Assert.AreEqual(k_FolderA.NormalizePath(), from);
                Assert.AreEqual(k_FolderB.NormalizePath(), to);
            };

            AssetManager.RenameAsset(k_FolderA, k_FolderB);

            Assert.AreEqual(1, callbackitCount, "Callback was fired 0 or to many times");
        }

        [TestMethod]
        public void DeleteFolder_WithAssetsInside_WillFireOnlyOneCallback()
        {
            AssetManager.CreateAsset(null, k_FolderA);

            var rec = AssetManager.CreateAsset(new Recording(), k_RecInFolderA);
            var fix = AssetManager.CreateAsset(TestFixtureManager.NewTestFixture().ToLightTestFixture(), k_FixInFolderA);

            var callbackitCount = 0;
            AssetManager.AssetDeleted += (path) =>
            {
                callbackitCount++;
                Assert.AreEqual(k_FolderA.NormalizePath(), path);
            };

            AssetManager.DeleteAsset(k_FolderA);

            Assert.AreEqual(1, callbackitCount, "Callback was fired 0 or to many times");
        }

        [TestMethod]
        public void RenameFolder_WithAssetsInside_DoesNotCallAssetDeletedCallbacks()
        {
            AssetManager.CreateAsset(null, k_FolderA);

            var rec = AssetManager.CreateAsset(new Recording(), k_RecInFolderA);
            var fix = AssetManager.CreateAsset(TestFixtureManager.NewTestFixture().ToLightTestFixture(), k_FixInFolderA);

            var callbackitCount = 0;
            AssetManager.AssetDeleted += _ => callbackitCount++;
            AssetManager.RenameAsset(k_FolderA, k_FolderB);

            Assert.AreEqual(0, callbackitCount, "Asset Deleted callback should not be fired");
        }

        [TestMethod]
        public void DeleteFolder_WillNotDeleteUnecessaryFoldersAndAssets_WhichStartWithSameString()
        {
            var folderA = "Assets\\FolderA";
            var folderB = "Assets\\FolderAB";
            var recording = "Assets\\FolderAWhichIsActuallyAsset.mrb";
            AssetManager.CreateAsset(null, folderA);
            AssetManager.CreateAsset(null, folderB);
            AssetManager.CreateAsset(new Recording(), recording);

            AssetManager.DeleteAsset(k_FolderA);

            Assert.IsNull(AssetManager.GetAsset(folderA), "FolderA should have been deleted");
            Assert.IsNotNull(AssetManager.GetAsset(folderB), "FolderA should have not been deleted");
            Assert.IsNotNull(AssetManager.GetAsset(recording), "FolderA should have not been deleted");
        }

        [TestMethod]
        public void CannotRenameFolder_ToBe_InsideItself()
        {
            var originalAsset = AssetManager.CreateAsset(null, k_FolderA);
            AssetManager.RenameAsset(k_FolderA, k_FolderANested);

            var oldAsset = AssetManager.GetAsset(k_FolderA);
            var newAsset = AssetManager.GetAsset(k_FolderANested);
            var warningCount = Logger.Instance.LogList.Count(log => log.LogType == LogType.Warning);

            Assert.AreEqual(originalAsset, oldAsset);
            Assert.IsNull(newAsset);
            Assert.AreEqual(1, warningCount);
        }

        private static void CreateDummyRecordingWithImporter(string path)
        {
            var importer = EditorAssetImporter.FromPath(path);
            importer.Value = new Recording(guid);
            importer.SaveAsset();
        }
    }
}
