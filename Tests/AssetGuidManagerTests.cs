﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Robot;
using RobotRuntime.Commands;
using Robot.Scripts;
using System.IO;
using RobotRuntime.Assets;
using System;

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

        [TestMethod]
        public void NewAssets_UponRefresh_AreAddedToGuidTable()
        {
            CreateDummyScriptWithImporter(k_ScriptAPath);
            CreateDummyScriptWithImporter(k_ScriptBPath);

            AssetManager.Instance.Refresh();

            Assert.AreEqual(2, AssetGuidManager.Instance.Paths.Count(), "Asset count missmatch");
            Assert.AreEqual(AssetManager.Instance.GetAsset(k_ScriptAPath).Guid, AssetGuidManager.Instance.GetGuid(k_ScriptAPath), "Asset guid missmatch");
            Assert.AreEqual(AssetManager.Instance.GetAsset(k_ScriptBPath).Guid, AssetGuidManager.Instance.GetGuid(k_ScriptBPath), "Asset guid missmatch");
        }

        [TestMethod]
        public void DeletedAssets_UponRefresh_AreNotRemovedFromGuidTable()
        {
            CreateDummyScriptWithImporter(k_ScriptAPath);
            CreateDummyScriptWithImporter(k_ScriptBPath);
            CreateDummyScriptWithImporter(k_ScriptCPath);
            AssetManager.Instance.Refresh();

            File.Delete(k_ScriptAPath);
            File.Delete(k_ScriptCPath);
            AssetManager.Instance.Refresh();

            Assert.AreEqual(1, AssetManager.Instance.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(3, AssetGuidManager.Instance.Paths.Count(), "Asset guid count missmatch");

            Assert.IsTrue(AssetGuidManager.Instance.ContainsValue(k_ScriptAPath), "Asset guid should still be in place");
            Assert.IsTrue(AssetGuidManager.Instance.ContainsValue(k_ScriptCPath), "Asset guid should still be in place");
            Assert.AreEqual(AssetManager.Instance.GetAsset(k_ScriptBPath).Guid, AssetGuidManager.Instance.GetGuid(k_ScriptBPath), "Asset guid missmatch");
        }

        [TestMethod]
        public void RenamedAssets_UponRefresh_AreRenamedInGuidTable()
        {
            var assetA = AssetManager.Instance.CreateAsset(new Script(), k_ScriptAPath);
            var assetB = AssetManager.Instance.CreateAsset(new Script(), k_ScriptBPath);

            File.Move(k_ScriptBPath, k_ScriptCPath);
            AssetManager.Instance.Refresh();

            Assert.AreEqual(2, AssetGuidManager.Instance.Paths.Count(), "Asset count missmatch");
            Assert.AreEqual(default(Guid), AssetGuidManager.Instance.GetGuid(k_ScriptBPath), "B asset was renamed");

            Assert.AreEqual(assetA.Guid, AssetGuidManager.Instance.GetGuid(k_ScriptAPath), "A path gives correct guid");
            Assert.AreEqual(assetB.Guid, AssetGuidManager.Instance.GetGuid(k_ScriptCPath), "C path gives correct guid");

            Assert.AreEqual(k_ScriptAPath, AssetGuidManager.Instance.GetPath(assetA.Guid), "A still has same path");
            Assert.AreEqual(k_ScriptCPath, AssetGuidManager.Instance.GetPath(assetB.Guid), "B gives different path");
        }

        [TestMethod]
        public void RenamedAssets_UponRefresh_AreGivenSameGuid()
        {
            var guidA = AssetManager.Instance.CreateAsset(new Script(), k_ScriptAPath).Guid;
            var guidB = AssetManager.Instance.CreateAsset(new Script(), k_ScriptBPath).Guid;

            CleanupScriptsDirectory();
            AssetManager.Instance.Refresh();

            CreateDummyScriptWithImporter(k_ScriptAPath);
            CreateDummyScriptWithImporter(k_ScriptCPath);
            AssetManager.Instance.Refresh();

            Assert.AreEqual(2, AssetManager.Instance.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(2, AssetGuidManager.Instance.Paths.Count(), "Guid-Path map count missmatch");


            Assert.AreEqual(guidA, AssetManager.Instance.GetAsset(k_ScriptAPath).Guid, "A asset has correct guid");
            Assert.AreEqual(guidB, AssetManager.Instance.GetAsset(k_ScriptCPath).Guid, "C asset has correct guid");

            Assert.AreEqual(guidA, AssetGuidManager.Instance.GetGuid(k_ScriptAPath), "A path gives correct guid");
            Assert.AreEqual(guidB, AssetGuidManager.Instance.GetGuid(k_ScriptCPath), "C path gives correct guid");
        }

        [TestMethod]
        public void Refresh_GivesAssetsSameGuid_IfTheyWereAlreadyKnown()
        {
            var guidA = AssetManager.Instance.CreateAsset(new Script(), k_ScriptAPath).Guid;
            var guidB = AssetManager.Instance.CreateAsset(new Script(), k_ScriptBPath).Guid;

            File.Delete(k_ScriptBPath);
            AssetManager.Instance.Refresh();

            CreateDummyScriptWithImporter(k_ScriptBPath);
            AssetManager.Instance.Refresh();

            Assert.AreEqual(2, AssetManager.Instance.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(2, AssetGuidManager.Instance.Paths.Count(), "Guid-Path map count missmatch");

            Assert.AreEqual(guidA, AssetManager.Instance.GetAsset(k_ScriptAPath).Guid, "A path gives correct guid");
            Assert.AreEqual(guidB, AssetManager.Instance.GetAsset(k_ScriptBPath).Guid, "B path gives correct guid");
        }

        [TestMethod]
        public void DeletingAndRestoringAsset_WillGiveItSameGuid_AsItHadOriginally()
        {
            var guidA = AssetManager.Instance.CreateAsset(new Script(), k_ScriptAPath).Guid;
            var guidB = AssetManager.Instance.CreateAsset(new Script(), k_ScriptBPath).Guid;

            AssetManager.Instance.DeleteAsset(k_ScriptBPath);

            CreateDummyScriptWithImporter(k_ScriptBPath);
            AssetManager.Instance.Refresh();

            Assert.AreEqual(2, AssetManager.Instance.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(2, AssetGuidManager.Instance.Paths.Count(), "Guid-Path map count missmatch");

            Assert.AreEqual(guidA, AssetManager.Instance.GetAsset(k_ScriptAPath).Guid, "A path gives correct guid");
            Assert.AreEqual(guidB, AssetManager.Instance.GetAsset(k_ScriptBPath).Guid, "B path gives correct guid");
        }

        [TestMethod]
        public void CreateAsset_AddsItToGuidTable()
        {
            var asset = AssetManager.Instance.CreateAsset(new Script(), k_ScriptAPath);
            var asset2 = AssetManager.Instance.CreateAsset(new Script(), k_ScriptBPath);

            Assert.AreEqual(2, AssetGuidManager.Instance.Paths.Count(), "Asset count missmatch");
            Assert.AreEqual(asset.Guid, AssetGuidManager.Instance.GetGuid(k_ScriptAPath), "Asset guid missmatch");
            Assert.AreEqual(asset2.Guid, AssetGuidManager.Instance.GetGuid(k_ScriptBPath), "Asset guid missmatch");
        }

        [TestMethod]
        public void DeleteAsset_DoesNotRemoveIt_FromGuidTable()
        {
            var asset = AssetManager.Instance.CreateAsset(new Script(), k_ScriptAPath);
            var asset2 = AssetManager.Instance.CreateAsset(new Script(), k_ScriptBPath);

            AssetManager.Instance.DeleteAsset(k_ScriptAPath);

            Assert.AreEqual(1, AssetManager.Instance.Assets.Count(), "Asset count missmatch");
            Assert.AreEqual(2, AssetGuidManager.Instance.Paths.Count(), "Asset guid count missmatch");

            Assert.IsTrue(AssetGuidManager.Instance.ContainsValue(k_ScriptAPath), "Asset guid should still be in place");
            Assert.AreEqual(asset2.Guid, AssetGuidManager.Instance.GetGuid(k_ScriptBPath), "Asset guid missmatch");
        }

        [TestMethod]
        public void RenameAsset_RenamesItInGuidTable()
        {
            var asset = AssetManager.Instance.CreateAsset(new Script(), k_ScriptAPath);
            var asset2 = AssetManager.Instance.CreateAsset(new Script(), k_ScriptBPath);

            AssetManager.Instance.RenameAsset(k_ScriptAPath, k_ScriptCPath);

            Assert.AreEqual(2, AssetGuidManager.Instance.Paths.Count(), "Asset count missmatch");
            Assert.IsFalse(AssetGuidManager.Instance.ContainsValue(k_ScriptAPath), "A path should be deleted");

            Assert.AreEqual(default(Guid), AssetGuidManager.Instance.GetGuid(k_ScriptAPath), "Asset guid missmatch");
            Assert.AreEqual(asset2.Guid, AssetGuidManager.Instance.GetGuid(k_ScriptBPath), "Asset guid missmatch");
            Assert.AreEqual(asset.Guid, AssetGuidManager.Instance.GetGuid(k_ScriptCPath), "Asset guid missmatch");
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
            CleanupScriptsDirectory();
            CleanupMetaDataDirectory();

            MouseRobot.Instance.SetupProjectPath(TempProjectPath);
            AssetManager.Instance.Refresh();
        }

        [TestCleanup]
        public void Cleanup()
        {
            CleanupScriptsDirectory();
            CleanupMetaDataDirectory();
            
            AssetGuidManager.Instance.LoadMetaFiles();
            AssetManager.Instance.Refresh();
        }

        private void CleanupMetaDataDirectory()
        {
            if (Directory.Exists(TempProjectPath + "\\" + AssetGuidManager.MetadataFolder))
            {
                DirectoryInfo di = new DirectoryInfo(TempProjectPath + "\\" + AssetGuidManager.MetadataFolder);
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
            }
        }

        private void CleanupScriptsDirectory()
        {
            if (Directory.Exists(TempProjectPath + "\\" + AssetManager.ScriptFolder))
            {
                DirectoryInfo di = new DirectoryInfo(TempProjectPath + "\\" + AssetManager.ScriptFolder);
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
            }
        }
    }
}