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

            Assert.AreEqual(2, m_ModifiedAssetCallbackFired, "Incorrect callback count");
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

            Assert.AreEqual(1, m_ModifiedAssetCallbackFired, "Incorrect callback count");
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

            Assert.AreEqual(2, m_ModifiedAssetCallbackFired, "Incorrect callback count");
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

            Assert.AreEqual(1, m_RenamedAssetCallbackFired, "Incorrect callback count");
            Assert.AreEqual(1, Collector.RenamedAssetPaths.Count, "Asset Path list count is incorrect");
            CollectionAssert.AreEquivalent(new[] { (From: k_RecPath, To: k_NewRecPath) }, Collector.RenamedAssetPaths);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void RenamingFolder_WillFire_RenamedAssetCallback(bool useFileSystem)
        {
            AssetManager.CreateAsset(null, k_TestFolderPath);
            AssetManager.CreateAsset(new Recording(), k_RecPath);
            ClearCollectorAndCallbacks();

            RenameAsset(k_TestFolderPath, k_NewTestFolderPath, useFileSystem);

            Assert.AreEqual(2, m_RenamedAssetCallbackFired, "Incorrect callback count");
            Assert.AreEqual(2, Collector.RenamedAssetPaths.Count, "Asset Path list count is incorrect");

            CollectionAssert.AreEquivalent(new[] {
                (From: k_RecPath, To: k_RecPathWithNewFolderPath),
                (From: k_TestFolderPath, To: k_NewTestFolderPath) }, 
                Collector.RenamedAssetPaths);
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
