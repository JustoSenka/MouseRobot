using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using RobotRuntime;
using RobotRuntime.Commands;
using RobotRuntime.Recordings;
using RobotRuntime.Tests;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity;

namespace Tests.Integration
{
    [TestFixture(true)]
    [TestFixture(false)]
    public class AssetTests : TestWithCleanup
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
        private const string k_FolderC = "Assets\\folderC\\";
        private const string k_FolderANested = "Assets\\folderA\\nestedFolder";

        private const string k_RecInFolderA = k_FolderA + "rec.mrb";
        private const string k_FixInFolderA = k_FolderA + "fix.mrt";

        private const string k_RecInFolderB = k_FolderB + "rec.mrb";
        private const string k_FixInFolderB = k_FolderB + "fix.mrt";

        private readonly static Guid guid = new Guid("12345678-9abc-def0-1234-567890123456");

        IAssetManager AssetManager;
        IHierarchyManager RecordingManager;
        ITestFixtureManager TestFixtureManager;

        private bool refreshBeforeTest;

        public AssetTests(bool refreshBeforeTest)
        {
            this.refreshBeforeTest = refreshBeforeTest;
        }

        [SetUp]
        public void Initialize()
        {
            TempProjectPath = TestUtils.GenerateProjectPath();
            InitializeNewProject(TempProjectPath);

            if (refreshBeforeTest)
                AssetManager.Refresh();
        }

        private void InitializeNewProject(string projectPath)
        {
            var container = TestUtils.ConstructContainerForTests(false);

            var ProjectManager = container.Resolve<IProjectManager>();
            AssetManager = container.Resolve<IAssetManager>();
            RecordingManager = container.Resolve<IHierarchyManager>();
            TestFixtureManager = container.Resolve<ITestFixtureManager>();

            ProjectManager.InitProject(projectPath).Wait();
        }

        [Test]
        public void TwoIdenticalAssets_HaveDifferentGuids_AndDifferentHashesDueToGuid()
        {
            var asset = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);

            Directory.CreateDirectory(new FileInfo(k_RecordingANestedPath).Directory.FullName);
            var asset2 = AssetManager.CreateAsset(new Recording(guid), k_RecordingANestedPath);

            Assert.AreNotEqual(asset.Hash, asset2.Hash, "Identical assets should have same hash");
            Assert.AreNotEqual(asset.Guid, asset2.Guid, "Identical assets should have different GUIDs");
        }

        [Test]
        public void AfterSavingAsset_ObjectGuidIsReplacedWith_AssetGuid()
        {
            var asset = AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);
            var rec = asset.Load<Recording>();

            Assert.AreEqual(asset.Guid, rec.Guid, "Asset and Recording guids should be the same");
            Assert.AreNotEqual(asset.Guid, guid, "Recording should now have a different guid after it was saved");
        }

        [Test]
        public void CreateAsset_CreatesFile_AndAddsToAssetManager()
        {
            var recording = new Recording(guid);
            AssetManager.CreateAsset(recording, k_RecordingAPath);

            Assert.IsTrue(File.Exists(k_RecordingAPath), "File was not created");
            Assert.AreEqual(AssetManager.GetAsset(k_RecordingAPath).Load<Recording>(), recording, "Asset value did not match the recording");
        }

        [Test]
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

        [Test]
        public void RenameAsset_RenamesFile_AndKeepsAllReferencesIntact()
        {
            var recording = new Recording(guid);
            var asset = AssetManager.CreateAsset(recording, k_RecordingAPath);

            AssetManager.RenameAsset(k_RecordingAPath, k_RecordingBPath);

            var newAsset = AssetManager.GetAsset(k_RecordingBPath);
            var newRecording = newAsset.Load<Recording>();

            Assert.AreEqual(asset, newAsset, "Asset reference should be the same after renaming asset");
            Assert.AreEqual(recording, newRecording, "Recording reference should be the same after renaming asset");
            Assert.IsFalse(File.Exists(k_RecordingAPath), "Old file should not exist anymore");
            Assert.IsTrue(File.Exists(k_RecordingBPath), "New file should exist on disk");
        }

        [Test]
        public void Refresh_WithFileBeingRenamedOnFileSystem_AcceptsRenameAndKeepsAllReferences()
        {
            var recording = new Recording(guid);
            var asset = AssetManager.CreateAsset(recording, k_RecordingAPath);

            File.Move(k_RecordingAPath, k_RecordingBPath);
            AssetManager.Refresh();

            var newAsset = AssetManager.GetAsset(k_RecordingBPath);
            var newRecording = newAsset.Load<Recording>();

            Assert.AreEqual(asset, newAsset, "Asset reference should be the same after renaming asset");
            Assert.AreEqual(recording, newRecording, "Recording reference should be the same after renaming asset");
            Assert.IsFalse(File.Exists(k_RecordingAPath), "Old file should not exist anymore");
            Assert.IsTrue(File.Exists(k_RecordingBPath), "New file should exist on disk");
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void CreateFolderAsset_AddsAssetToManager_AndCreatesDirAtDisk()
        {
            var path = "Assets\\someFolder";
            AssetManager.CreateAsset(null, path);

            Assert.AreEqual(2, AssetManager.Assets.Count(), "Two dir assets should be known");
            Assert.AreEqual(path.NormalizePath(), AssetManager.GetAsset(path).Load<string>().NormalizePath());
        }

        [Test]
        public void CreateAsset_WhenFolderDoesNotExist_ReturnsNull()
        {
            var path = "Assets\\non existant folder\\rec.mrb";
            var res = AssetManager.CreateAsset(new Recording(), path);

            Assert.IsNull(res, "Cannot create assets when folder does not exist");
        }

        [Test]
        public void CreateFolderAsset_WhenFolderDoesNotExist_ReturnsNull()
        {
            var path = "Assets\\non existant folder\\someFolder";
            var res = AssetManager.CreateAsset(null, path);

            Assert.IsNull(res, "Cannot create assets when folder does not exist");
        }

        [Test]
        public void AssetManager_CanAddTwoAssets_WithSameName_ButDifferentExtension()
        {
            AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);
            AssetManager.CreateAsset(TestFixtureManager.NewTestFixture().ToLightTestFixture(), k_FixtureAPath);

            AssetManager.Refresh();

            Assert.AreEqual(3, AssetManager.Assets.Count(), "Two assets + directory should be found");
        }

        [Test]
        public void AssetManager_CanRefreshTwoAssets_WithSameName_ButDifferentExtension()
        {
            CreateDummyRecordingWithImporter(k_RecordingAPath);
            AssetManager.CreateAsset(TestFixtureManager.NewTestFixture().ToLightTestFixture(), k_FixtureAPath);

            Assert.AreEqual(2, AssetManager.Assets.Count(), "One assets + directory should be found");
            AssetManager.Refresh();

            Assert.AreEqual(3, AssetManager.Assets.Count(), "Two assets + directory should be found");
        }

        [Test]
        public void AssetManager_CanReturnTwoAssets_WithSameName_ButDifferentExtension()
        {
            AssetManager.CreateAsset(new Recording(guid), k_RecordingAPath);
            AssetManager.CreateAsset(TestFixtureManager.NewTestFixture().ToLightTestFixture(), k_FixtureAPath);

            Assert.AreEqual(typeof(Recording), AssetManager.GetAsset(k_RecordingAPath).HoldsType());
            Assert.AreEqual(typeof(LightTestFixture), AssetManager.GetAsset(k_FixtureAPath).HoldsType());
        }

        [Test]
        public void RenameFolder_WithAssetsInside_WillKeepAllGuids()
        {
            Directory.CreateDirectory(k_FolderA);

            var rec = AssetManager.CreateAsset(new Recording(), k_RecInFolderA);
            var fix = AssetManager.CreateAsset(TestFixtureManager.NewTestFixture().ToLightTestFixture(), k_FixInFolderA);

            Directory.Move(k_FolderA, k_FolderB);
            AssetManager.Refresh();

            Assert.AreEqual(4, AssetManager.Assets.Count(), "Asset count missmatch");

            Assert.AreEqual(typeof(Recording), AssetManager.GetAsset(k_RecInFolderB).HoldsType());
            Assert.AreEqual(typeof(LightTestFixture), AssetManager.GetAsset(k_FixInFolderB).HoldsType());

            Assert.AreEqual(rec.Guid, AssetManager.GetAsset(k_RecInFolderB).Guid);
            Assert.AreEqual(fix.Guid, AssetManager.GetAsset(k_FixInFolderB).Guid);
        }

        [Test]
        public void RenameFolder_FromAssetManager_WithAssetsInside_WillKeepAllGuids()
        {
            AssetManager.CreateAsset(null, k_FolderA);

            var rec = AssetManager.CreateAsset(new Recording(), k_RecInFolderA);
            var fix = AssetManager.CreateAsset(TestFixtureManager.NewTestFixture().ToLightTestFixture(), k_FixInFolderA);

            AssetManager.RenameAsset(k_FolderA, k_FolderB);

            Assert.AreEqual(4, AssetManager.Assets.Count(), "Asset count missmatch");

            Assert.AreEqual(typeof(Recording), AssetManager.GetAsset(k_RecInFolderB).HoldsType());
            Assert.AreEqual(typeof(LightTestFixture), AssetManager.GetAsset(k_FixInFolderB).HoldsType());

            Assert.AreEqual(rec.Guid, AssetManager.GetAsset(k_RecInFolderB).Guid);
            Assert.AreEqual(fix.Guid, AssetManager.GetAsset(k_FixInFolderB).Guid);
        }

        [Test]
        public void RenameFolder_WithAssetsInside_WillFireCallbackForAllAssets()
        {
            AssetManager.CreateAsset(null, k_FolderA);

            var rec = AssetManager.CreateAsset(new Recording(), k_RecInFolderA);
            var fix = AssetManager.CreateAsset(TestFixtureManager.NewTestFixture().ToLightTestFixture(), k_FixInFolderA);

            var callbackitCount = 0;
            var list = new List<(string From, string To)>();
            AssetManager.AssetRenamed += (from, to) =>
            {
                callbackitCount++;
                list.Add((from, to));
            };

            AssetManager.RenameAsset(k_FolderA, k_FolderB);

            Assert.AreEqual(3, callbackitCount, "Callback was fired 0 or to many times");
            CollectionAssert.AreEquivalent(new[] {
                (k_FolderA.NormalizePath(), k_FolderB.NormalizePath()),
                (k_RecInFolderA, k_RecInFolderB),
                (k_FixInFolderA, k_FixInFolderB),
            }, list);
        }

        [Test]
        public void RenameFolder_WithAssetsInside_WillRenameAllAssetsAsWell()
        {
            AssetManager.CreateAsset(null, k_FolderA);

            var rec = AssetManager.CreateAsset(new Recording(), k_RecInFolderA);
            var fix = AssetManager.CreateAsset(TestFixtureManager.NewTestFixture().ToLightTestFixture(), k_FixInFolderA);

            AssetManager.RenameAsset(k_FolderA, k_FolderB);

            CollectionAssert.AreEquivalent(
                new[] { "Assets", k_FolderB.NormalizePath(), k_RecInFolderB, k_FixInFolderB },
                AssetManager.Assets.Select(a => a.Path));
        }

        [Test]
        public void DeleteFolder_WithAssetsInside_WillFireCallbackForAllAssets()
        {
            AssetManager.CreateAsset(null, k_FolderA);

            var rec = AssetManager.CreateAsset(new Recording(), k_RecInFolderA);
            var fix = AssetManager.CreateAsset(TestFixtureManager.NewTestFixture().ToLightTestFixture(), k_FixInFolderA);

            var callbackitCount = 0;
            var list = new List<string>();
            AssetManager.AssetDeleted += (path) =>
            {
                callbackitCount++;
                list.Add(path);
            };

            AssetManager.DeleteAsset(k_FolderA);

            Assert.AreEqual(3, callbackitCount, "Callback was fired 0 or to many times");
            CollectionAssert.AreEquivalent(new[] { k_FolderA.NormalizePath(), k_RecInFolderA, k_FixInFolderA }, list);
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        [TestCase(true, "FolderA", "FolderB")]
        [TestCase(true, "Fold", "Folder")]
        [TestCase(true, "Folder", "Fold")]
        [TestCase(false, "FolderA", "FolderB")]
        [TestCase(false, "Fold", "Folder")]
        [TestCase(false, "Folder", "Fold")]
        public void RenameFolder_WithAssetsInside_RenamesCorrectAssets(bool renameViaFileManager, string folderA, string folderB)
        {
            folderA = Path.Combine("Assets", folderA);
            folderB = Path.Combine("Assets", folderB);
            var folderC = folderB + "e"; // some additional character which would result in a folder with similar name

            var a2 = AssetManager.CreateAsset(null, folderB);
            var a1 = AssetManager.CreateAsset(null, folderA);
            var a3 = AssetManager.CreateAsset(new Recording(), Path.Combine(folderA, "rec.mrb"));
            var a4 = AssetManager.CreateAsset(new Recording(), Path.Combine(folderB, "rec.mrb"));
            var a5 = AssetManager.CreateAsset(null, Path.Combine(folderA, "new fold"));
            var a6 = AssetManager.CreateAsset(null, Path.Combine(folderB, "new fold"));

            RenameAsset(renameViaFileManager, folderB, folderC);

            Assert.AreEqual(7, AssetManager.Assets.Count(), "Asset count missmatch");

            Assert.IsNotNull(AssetManager.GetAsset(folderA), "FolderA should be available");
            Assert.IsNotNull(AssetManager.GetAsset(folderC), "FolderC should be available");
            Assert.AreEqual(a3.Guid, AssetManager.GetAsset(Path.Combine(folderA, "rec.mrb")).Guid, "Asset inside FolderA guid missmatch");
            Assert.AreEqual(a4.Guid, AssetManager.GetAsset(Path.Combine(folderC, "rec.mrb")).Guid, "Asset inside FolderC guid missmatch");
            Assert.IsNotNull(AssetManager.GetAsset(Path.Combine(folderA, "new fold")), "Folder inside FolderA should be available");
            Assert.IsNotNull(AssetManager.GetAsset(Path.Combine(folderC, "new fold")), "Folder inside FolderB should be available");

            Assert.IsNull(AssetManager.GetAsset(folderB), "folderB should not exists");
            Assert.IsNull(AssetManager.GetAsset(Path.Combine(folderB, "new fold")), "folderB should not have assets inside");
        }

        [Test]
        [TestCase("FolderA", "FolderB")]
        [TestCase("Fold", "Folder")]
        [TestCase("Folder", "Fold")]
        public void StartingApp_WithRenamedFolder_WithAssetsInside_WillDetectRenameCorrectly(string folderA, string folderB)
        {
            folderA = Path.Combine("Assets", folderA);
            folderB = Path.Combine("Assets", folderB);
            var folderC = folderB + "e"; // some additional character which would result in a folder with similar name

            var a2 = AssetManager.CreateAsset(null, folderB);
            var a1 = AssetManager.CreateAsset(null, folderA);
            var a3 = AssetManager.CreateAsset(new Recording(), Path.Combine(folderA, "rec.mrb"));
            var a4 = AssetManager.CreateAsset(new Recording(), Path.Combine(folderB, "rec.mrb"));

            // Copying to new place, since old asset manager still lives and will continue to monitor old path.
            // If I rename now, old asset manager will detect it
            var newProjectPath = Path.Combine(TempProjectPath, "newProject");
            Paths.CopyDirectories(TempProjectPath, newProjectPath);

            RenameAsset(true, Path.Combine(newProjectPath, folderB), Path.Combine(newProjectPath, folderC));

            InitializeNewProject(newProjectPath);

            Assert.AreEqual(5, AssetManager.Assets.Count(), "Asset count missmatch");

            Assert.IsNotNull(AssetManager.GetAsset(folderA), "FolderA should be available");
            Assert.IsNotNull(AssetManager.GetAsset(folderC), "FolderC should be available");
            Assert.AreEqual(a3.Guid, AssetManager.GetAsset(Path.Combine(folderA, "rec.mrb")).Guid, "Asset inside FolderA guid missmatch");
            Assert.AreEqual(a4.Guid, AssetManager.GetAsset(Path.Combine(folderC, "rec.mrb")).Guid, "Asset inside FolderC guid missmatch");
        }

        [Test]
        [TestCase("FolderA", "rec.mrb")]
        public void SecondRefresh_WithDeletedAssets_RemovesThemFromManager(string folderA, string assetName)
        {
            folderA = Path.Combine("Assets", folderA);
            AssetManager.Refresh();

            var a1 = AssetManager.CreateAsset(null, folderA);
            var a2 = AssetManager.CreateAsset(new Recording(guid), Path.Combine(folderA, assetName));

            Directory.Delete(folderA, true);
            AssetManager.Refresh();

            Assert.IsNull(AssetManager.GetAsset(a1.Path), "Folder asset should not exist");
            Assert.IsNull(AssetManager.GetAsset(a2.Path), "Recording asset should not exist");
            Assert.AreEqual(1, AssetManager.Assets.Count()); // only Assets folder is registered
        }

        private void RenameAsset(bool renameViaFileManager, string from, string to)
        {
            if (renameViaFileManager)
            {
                if (Paths.IsDirectory(from))
                    Directory.Move(from, to);

                else
                    File.Move(from, to);

                AssetManager.Refresh();
            }
            else
                AssetManager.RenameAsset(from, to);
        }

        private static void CreateDummyRecordingWithImporter(string path)
        {
            var importer = EditorAssetImporter.FromPath(path);
            importer.Value = new Recording(guid);
            importer.SaveAsset();
        }
    }
}
