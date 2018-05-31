using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Robot;
using Robot.Scripts;
using System.IO;
using System;
using Robot.Abstractions;
using RobotRuntime.Abstractions;
using Unity;
using RobotRuntime.Utils;
using Unity.Lifetime;
using RobotRuntime;

namespace Tests
{
    [TestClass]
    public class AssetGuidManagerTests
    {
        private string TempProjectPath
        {
            get
            {
                return System.IO.Path.GetTempPath() + "MProject";
            }
        }

        private const string k_ScriptAPath = "Scripts\\A.mrb";
        private const string k_ScriptBPath = "Scripts\\B.mrb";
        private const string k_ScriptCPath = "Scripts\\C.mrb";

        IMouseRobot MouseRobot;
        IAssetManager AssetManager;
        IAssetGuidManager AssetGuidManager;

        [TestMethod]
        public void NewAssets_UponRefresh_AreAddedToGuidTable()
        {
            CreateDummyScriptWithImporter(k_ScriptAPath);
            CreateDummyScriptWithImporter(k_ScriptBPath);

            AssetManager.Refresh();

            Assert.AreEqual(2, AssetGuidManager.Paths.Count(), "Asset count missmatch");
            Assert.AreEqual(AssetManager.GetAsset(k_ScriptAPath).Guid, AssetGuidManager.GetGuid(k_ScriptAPath), "Asset guid missmatch");
            Assert.AreEqual(AssetManager.GetAsset(k_ScriptBPath).Guid, AssetGuidManager.GetGuid(k_ScriptBPath), "Asset guid missmatch");
        }

        [TestMethod]
        public void DeletedAssets_UponRefresh_AreNotRemovedFromGuidTable()
        {
            CreateDummyScriptWithImporter(k_ScriptAPath);
            CreateDummyScriptWithImporter(k_ScriptBPath);
            CreateDummyScriptWithImporter(k_ScriptCPath);
            AssetManager.Refresh();

            File.Delete(k_ScriptAPath);
            File.Delete(k_ScriptCPath);
            AssetManager.Refresh();

            Assert.AreEqual(1, AssetManager.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(3, AssetGuidManager.Paths.Count(), "Asset guid count missmatch");

            Assert.IsTrue(AssetGuidManager.ContainsValue(k_ScriptAPath), "Asset guid should still be in place");
            Assert.IsTrue(AssetGuidManager.ContainsValue(k_ScriptCPath), "Asset guid should still be in place");
            Assert.AreEqual(AssetManager.GetAsset(k_ScriptBPath).Guid, AssetGuidManager.GetGuid(k_ScriptBPath), "Asset guid missmatch");
        }

        [TestMethod]
        public void RenamedAssets_UponRefresh_AreRenamedInGuidTable()
        {
            var assetA = AssetManager.CreateAsset(new Script(), k_ScriptAPath);
            var assetB = AssetManager.CreateAsset(new Script(), k_ScriptBPath);

            File.Move(k_ScriptBPath, k_ScriptCPath);
            AssetManager.Refresh();

            Assert.AreEqual(2, AssetGuidManager.Paths.Count(), "Asset count missmatch");
            Assert.AreEqual(default(Guid), AssetGuidManager.GetGuid(k_ScriptBPath), "B asset was renamed");

            Assert.AreEqual(assetA.Guid, AssetGuidManager.GetGuid(k_ScriptAPath), "A path gives correct guid");
            Assert.AreEqual(assetB.Guid, AssetGuidManager.GetGuid(k_ScriptCPath), "C path gives correct guid");

            Assert.AreEqual(k_ScriptAPath, AssetGuidManager.GetPath(assetA.Guid), "A still has same path");
            Assert.AreEqual(k_ScriptCPath, AssetGuidManager.GetPath(assetB.Guid), "B gives different path");
        }

        [TestMethod]
        public void RenamedAssets_UponRefresh_AreGivenSameGuid()
        {
            var guidA = AssetManager.CreateAsset(new Script(), k_ScriptAPath).Guid;
            var guidB = AssetManager.CreateAsset(new Script(), k_ScriptBPath).Guid;

            CleanupScriptsDirectory();
            AssetManager.Refresh();

            CreateDummyScriptWithImporter(k_ScriptAPath);
            CreateDummyScriptWithImporter(k_ScriptCPath);
            AssetManager.Refresh();

            Assert.AreEqual(2, AssetManager.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(2, AssetGuidManager.Paths.Count(), "Guid-Path map count missmatch");


            Assert.AreEqual(guidA, AssetManager.GetAsset(k_ScriptAPath).Guid, "A asset has correct guid");
            Assert.AreEqual(guidB, AssetManager.GetAsset(k_ScriptCPath).Guid, "C asset has correct guid");

            Assert.AreEqual(guidA, AssetGuidManager.GetGuid(k_ScriptAPath), "A path gives correct guid");
            Assert.AreEqual(guidB, AssetGuidManager.GetGuid(k_ScriptCPath), "C path gives correct guid");
        }

        [TestMethod]
        public void Refresh_GivesAssetsSameGuid_IfTheyWereAlreadyKnown()
        {
            var guidA = AssetManager.CreateAsset(new Script(), k_ScriptAPath).Guid;
            var guidB = AssetManager.CreateAsset(new Script(), k_ScriptBPath).Guid;

            File.Delete(k_ScriptBPath);
            AssetManager.Refresh();

            CreateDummyScriptWithImporter(k_ScriptBPath);
            AssetManager.Refresh();

            Assert.AreEqual(2, AssetManager.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(2, AssetGuidManager.Paths.Count(), "Guid-Path map count missmatch");

            Assert.AreEqual(guidA, AssetManager.GetAsset(k_ScriptAPath).Guid, "A path gives correct guid");
            Assert.AreEqual(guidB, AssetManager.GetAsset(k_ScriptBPath).Guid, "B path gives correct guid");
        }

        [TestMethod]
        public void DeletingAndRestoringAsset_WillGiveItSameGuid_AsItHadOriginally()
        {
            var guidA = AssetManager.CreateAsset(new Script(), k_ScriptAPath).Guid;
            var guidB = AssetManager.CreateAsset(new Script(), k_ScriptBPath).Guid;

            AssetManager.DeleteAsset(k_ScriptBPath);

            CreateDummyScriptWithImporter(k_ScriptBPath);
            AssetManager.Refresh();

            Assert.AreEqual(2, AssetManager.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(2, AssetGuidManager.Paths.Count(), "Guid-Path map count missmatch");

            Assert.AreEqual(guidA, AssetManager.GetAsset(k_ScriptAPath).Guid, "A path gives correct guid");
            Assert.AreEqual(guidB, AssetManager.GetAsset(k_ScriptBPath).Guid, "B path gives correct guid");
        }

        [TestMethod]
        public void CreateAsset_AddsItToGuidTable()
        {
            var asset = AssetManager.CreateAsset(new Script(), k_ScriptAPath);
            var asset2 = AssetManager.CreateAsset(new Script(), k_ScriptBPath);

            Assert.AreEqual(2, AssetGuidManager.Paths.Count(), "Asset count missmatch");
            Assert.AreEqual(asset.Guid, AssetGuidManager.GetGuid(k_ScriptAPath), "Asset guid missmatch");
            Assert.AreEqual(asset2.Guid, AssetGuidManager.GetGuid(k_ScriptBPath), "Asset guid missmatch");
        }

        [TestMethod]
        public void DeleteAsset_DoesNotRemoveIt_FromGuidTable()
        {
            var asset = AssetManager.CreateAsset(new Script(), k_ScriptAPath);
            var asset2 = AssetManager.CreateAsset(new Script(), k_ScriptBPath);

            AssetManager.DeleteAsset(k_ScriptAPath);

            Assert.AreEqual(1, AssetManager.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(2, AssetGuidManager.Paths.Count(), "Asset guid count missmatch");

            Assert.IsTrue(AssetGuidManager.ContainsValue(k_ScriptAPath), "Asset guid should still be in place");
            Assert.AreEqual(asset2.Guid, AssetGuidManager.GetGuid(k_ScriptBPath), "Asset guid missmatch");
        }

        [TestMethod]
        public void RenameAsset_RenamesItInGuidTable()
        {
            var asset = AssetManager.CreateAsset(new Script(), k_ScriptAPath);
            var asset2 = AssetManager.CreateAsset(new Script(), k_ScriptBPath);

            AssetManager.RenameAsset(k_ScriptAPath, k_ScriptCPath);

            Assert.AreEqual(2, AssetGuidManager.Paths.Count(), "Asset count missmatch");
            Assert.IsFalse(AssetGuidManager.ContainsValue(k_ScriptAPath), "A path should be deleted");

            Assert.AreEqual(default(Guid), AssetGuidManager.GetGuid(k_ScriptAPath), "Asset guid missmatch");
            Assert.AreEqual(asset2.Guid, AssetGuidManager.GetGuid(k_ScriptBPath), "Asset guid missmatch");
            Assert.AreEqual(asset.Guid, AssetGuidManager.GetGuid(k_ScriptCPath), "Asset guid missmatch");
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
            RobotRuntime.Program.RegisterInterfaces(container);
            Robot.Program.RegisterInterfaces(container);
            container.RegisterType<ILogger, FakeLogger>(new ContainerControlledLifetimeManager());
            Logger.Instance = container.Resolve<ILogger>();

            MouseRobot = container.Resolve<IMouseRobot>();
            var ProjectManager = container.Resolve<IProjectManager>();
            AssetManager = container.Resolve<IAssetManager>();
            AssetGuidManager = container.Resolve<IAssetGuidManager>();

            ProjectManager.InitProject(TempProjectPath);

            CleanupScriptsDirectory();
            CleanupMetaDataDirectory();

            AssetGuidManager.LoadMetaFiles();
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
            if (Directory.Exists(Paths.MetadataPath))
            {
                DirectoryInfo di = new DirectoryInfo(Paths.MetadataPath);
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
            }
        }

        private void CleanupScriptsDirectory()
        {
            if (Directory.Exists(TempProjectPath + "\\" + Paths.ScriptFolder))
            {
                DirectoryInfo di = new DirectoryInfo(TempProjectPath + "\\" + Paths.ScriptFolder);
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
            }
        }
    }
}
