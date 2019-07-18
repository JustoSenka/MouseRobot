using NUnit.Framework;
using Robot;
using Robot.Abstractions;
using RobotRuntime.IO;
using RobotRuntime.Recordings;
using RobotRuntime.Utils;
using System.IO;
using Unity;

namespace Tests.Unit
{
    [TestFixture]
    public class ModifiedAssetCollectorTests : TestWithCleanup
    {
        private string TempProjectPath;

        private const string k_TestFolderPath = "Assets\\TestFolder";
        private const string k_RecPath = "Assets\\TestFolder\\rec.mrb";

        private const string k_NewTestFolderPath = "Assets\\new-TestFolder";
        private const string k_NewRecPath = "Assets\\TestFolder\\new-rec.mrb";
        private const string k_RecPathWithNewFolderPath = "Assets\\new-TestFolder\\rec.mrb";

        IAssetManager AssetManager;
        ITestRunnerManager TestRunnerManager;
        ITestFixtureManager TestFixtureManager;
        IModifiedAssetCollector Collector;

        private int m_ModifiedAssetCallbackFired = 0;
        private int m_RenamedAssetCallbackFired = 0;

        [SetUp]
        public void Initialize()
        {
            TempProjectPath = TestUtils.GenerateProjectPath();
            var container = TestUtils.ConstructContainerForTests();

            var ProjectManager = container.Resolve<IProjectManager>();
            AssetManager = container.Resolve<IAssetManager>();
            TestRunnerManager = container.Resolve<ITestRunnerManager>();
            TestFixtureManager = container.Resolve<ITestFixtureManager>();
            Collector = container.Resolve<IModifiedAssetCollector>();

            ProjectManager.InitProject(TempProjectPath);

            Collector.AutoClear = false;
            ClearCollectorAndCallbacks();
            Collector.AssetsModified += _ => m_ModifiedAssetCallbackFired++;
            Collector.AssetsRenamed += _ => m_RenamedAssetCallbackFired++;
        }

        private void ClearCollectorAndCallbacks()
        {
            Collector.Clear();
            m_ModifiedAssetCallbackFired = 0;
            m_RenamedAssetCallbackFired = 0;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AddingAsset_WillFire_ModifiedAssetCallback(bool useFileSystem)
        {
            AssetManager.CreateAsset(null, k_TestFolderPath);
            CreateAndSaveNewRecording(k_RecPath, useFileSystem);

            Assert.IsTrue(m_ModifiedAssetCallbackFired >= 1, "Incorrect callback count");
            Assert.AreEqual(2, Collector.ModifiedAssetPaths.Count, "Asset Path list count is incorrect");
            CollectionAssert.AreEquivalent(new[] { k_TestFolderPath, k_RecPath }, Collector.ModifiedAssetPaths);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DeletingAsset_WillFire_ModifiedAssetCallback(bool useFileSystem)
        {
            AssetManager.CreateAsset(null, k_TestFolderPath);
            AssetManager.CreateAsset(new Recording(), k_RecPath);
            ClearCollectorAndCallbacks();

            DeleteAsset(k_RecPath, useFileSystem);

            Assert.IsTrue(m_ModifiedAssetCallbackFired >= 1, "Incorrect callback count");
            Assert.AreEqual(1, Collector.ModifiedAssetPaths.Count, "Asset Path list count is incorrect");
            CollectionAssert.AreEquivalent(new[] { k_RecPath }, Collector.ModifiedAssetPaths);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DeletingFolderAsset_WillFire_ModifiedAssetCallback(bool useFileSystem)
        {
            AssetManager.CreateAsset(null, k_TestFolderPath);
            AssetManager.CreateAsset(new Recording(), k_RecPath);
            ClearCollectorAndCallbacks();

            DeleteAsset(k_TestFolderPath, useFileSystem);

            Assert.IsTrue(m_ModifiedAssetCallbackFired >= 1, "Incorrect callback count");
            Assert.AreEqual(2, Collector.ModifiedAssetPaths.Count, "Asset Path list count is incorrect");
            CollectionAssert.AreEquivalent(new[] { k_TestFolderPath, k_RecPath }, Collector.ModifiedAssetPaths);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void RenamingAsset_WillFire_RenamedAssetCallback(bool useFileSystem)
        {
            AssetManager.CreateAsset(null, k_TestFolderPath);
            AssetManager.CreateAsset(new Recording(), k_RecPath);
            ClearCollectorAndCallbacks();

            RenameAsset(k_RecPath, k_NewRecPath, useFileSystem);

            Assert.IsTrue(m_RenamedAssetCallbackFired >= 1, "Incorrect callback count");
            Assert.AreEqual(1, Collector.RenamedAssetPaths.Count, "Asset Path list count is incorrect");
            CollectionAssert.AreEquivalent(new[] { (From: k_RecPath, To: k_NewRecPath) }, Collector.RenamedAssetPaths);
        }

        [Test]
        public void RenamingFolder_WillFire_RenamedAssetCallback_ViaAssetManager()
        {
            AssetManager.CreateAsset(null, k_TestFolderPath);
            AssetManager.CreateAsset(new Recording(), k_RecPath);
            ClearCollectorAndCallbacks();

            RenameAsset(k_TestFolderPath, k_NewTestFolderPath, false);

            Assert.IsTrue(m_RenamedAssetCallbackFired >= 1, "Incorrect callback count");
            Assert.AreEqual(2, Collector.RenamedAssetPaths.Count, "Asset Path list count is incorrect");
            Assert.AreEqual(0, Collector.ModifiedAssetPaths.Count, "Modified path list should be empty");

            CollectionAssert.AreEquivalent(new[] {
                (From: k_RecPath, To: k_RecPathWithNewFolderPath),
                (From: k_TestFolderPath, To: k_NewTestFolderPath) },
                Collector.RenamedAssetPaths);
        }

        [Test]
        public void RenamingFolder_WillFire_RenamedAssetCallback_ViaFileSystem()
        {
            AssetManager.CreateAsset(null, k_TestFolderPath);
            AssetManager.CreateAsset(new Recording(), k_RecPath);
            ClearCollectorAndCallbacks();

            RenameAsset(k_TestFolderPath, k_NewTestFolderPath, true);

            Assert.IsTrue(m_RenamedAssetCallbackFired >= 1, "Incorrect callback count");
            Assert.AreEqual(1, Collector.RenamedAssetPaths.Count, "Asset Path list count is incorrect");

            CollectionAssert.AreEquivalent(new[] { (From: k_RecPath, To: k_RecPathWithNewFolderPath) }, Collector.RenamedAssetPaths);

            // 2019-7-18
            // Folders actually do not get called with renamed callback, they are always modified
            // Etc deleted and added. That's because it's impossible to know if doing it via file system
            // The Rec is also modified for unknown reasons, seems that the rec hash changes after moving it to different folder
            // Or maybe it is incorrect upon initialization, but it gets both callbacks, renamed and modified
            // If the hash changes, technically it is correct, it was both modified and renamed. So let it be like that
            // It should not do any harm, but other systems should be aware that modified asset could be renamed at the same time,
            // so they need to update the correct one, with correct path/name
            // If rec disappears at some point from Modified list, it should also be fine
            CollectionAssert.AreEquivalent(new[] { k_TestFolderPath, k_NewTestFolderPath, k_RecPathWithNewFolderPath }, Collector.ModifiedAssetPaths);
        }

        private void CreateAndSaveNewRecording(string path, bool useFileSystem)
        {
            if (useFileSystem)
            {
                new YamlRecordingIO().SaveObject(path, new Recording());
                AssetManager.Refresh();
            }
            else
                AssetManager.CreateAsset(new Recording(), path);
        }

        private void DeleteAsset(string path, bool useFileSystem)
        {
            if (useFileSystem)
            {
                if (Paths.IsDirectory(path))
                    Directory.Delete(path, true);
                else
                    File.Delete(path);
                AssetManager.Refresh();
            }
            else
                AssetManager.DeleteAsset(path);
        }

        private void RenameAsset(string path, string newPath, bool useFileSystem)
        {
            if (useFileSystem)
            {
                if (Paths.IsDirectory(path))
                    Directory.Move(path, newPath);
                else
                    File.Move(path, newPath);

                AssetManager.Refresh();
            }
            else
                AssetManager.RenameAsset(path, newPath);
        }
    }
}
