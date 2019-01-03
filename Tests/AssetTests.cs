using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Robot;
using RobotRuntime.Commands;
using Robot.Recordings;
using System.IO;
using Robot.Abstractions;
using Unity;
using RobotRuntime.Utils;
using Unity.Lifetime;
using RobotRuntime.Abstractions;
using RobotRuntime;
using RobotRuntime.Recordings;
using System;

namespace Tests
{
    [TestClass]
    public class AssetTests
    {
        private string TempProjectPath
        {
            get
            {
                return System.IO.Path.GetTempPath() + "\\MProject";
            }
        }

        private const string k_ScriptAPath = "Scripts\\A.mrb";
        private const string k_ScriptBPath = "Scripts\\B.mrb";
        private const string k_ScriptCPath = "Scripts\\C.mrb";
        private const string k_ScriptDPath = "Scripts\\D.mrb";

        private readonly static Guid guid = new Guid("12345678-9abc-def0-1234-567890123456");

        IMouseRobot MouseRobot;
        IAssetManager AssetManager;
        IHierarchyManager ScriptManager;

        [TestMethod]
        public void TwoIdenticalAssets_HaveTheSameHash_ButDifferentGuids()
        {
            var asset = AssetManager.CreateAsset(new Recording(guid), k_ScriptAPath);
            var asset2 = AssetManager.CreateAsset(new Recording(guid), k_ScriptBPath);

            Assert.AreEqual(asset.Hash, asset2.Hash, "Identical assets should have same hash");
            Assert.AreNotEqual(asset.Guid, asset2.Guid, "Identical assets should have different GUIDs");
        }

        [TestMethod]
        public void CreateAsset_CreatesFile_AndAddsToAssetManager()
        {
            var script = new Recording(guid);
            AssetManager.CreateAsset(script, k_ScriptAPath);

            Assert.IsTrue(File.Exists(k_ScriptAPath), "File was not created");
            Assert.AreEqual(AssetManager.GetAsset(k_ScriptAPath).Importer.Load<Recording>(), script, "Asset value did not match the script");
        }

        [TestMethod]
        public void ReloadScript_UpdatesScriptRef_FromFile()
        {
            var script = ScriptManager.LoadedScripts[0];
            ScriptManager.SaveScript(script, k_ScriptAPath);
            script.AddCommand(new CommandSleep(5));

            var script2 = ScriptManager.LoadScript(k_ScriptAPath);

            Assert.AreNotEqual(script, script2, "Reloading script should give new reference type object");
            Assert.AreEqual(script2, ScriptManager.LoadedScripts[0], "Scripts from assets and in script manager should be the same");
            Assert.AreEqual(0, script2.Commands.Count(), "Script should not have commands");
        }

        [TestMethod]
        public void RenameAsset_RenamesFile_AndKeepsAllReferencesIntact()
        {
            var script = new Recording(guid);
            var asset = AssetManager.CreateAsset(script, k_ScriptAPath);

            AssetManager.RenameAsset(k_ScriptAPath, k_ScriptBPath);

            var newAsset = AssetManager.GetAsset(k_ScriptBPath);
            var newScript = newAsset.Importer.Load<Recording>();

            Assert.AreEqual(asset, newAsset, "Asset reference should be the same after renaming asset");
            Assert.AreEqual(script, newScript, "Script reference should be the same after renaming asset");
            Assert.IsFalse(File.Exists(k_ScriptAPath), "Old file should not exist anymore");
            Assert.IsTrue(File.Exists(k_ScriptBPath), "New file should exist on disk");
        }

        [TestMethod]
        public void Refresh_WithFileBeingRenamedOnFileSystem_AcceptsRenameAndKeepsAllReferences()
        {
            var script = new Recording(guid);
            var asset = AssetManager.CreateAsset(script, k_ScriptAPath);

            File.Move(k_ScriptAPath, k_ScriptBPath);
            AssetManager.Refresh();

            var newAsset = AssetManager.GetAsset(k_ScriptBPath);
            var newScript = newAsset.Importer.Load<Recording>();

            Assert.AreEqual(asset, newAsset, "Asset reference should be the same after renaming asset");
            Assert.AreEqual(script, newScript, "Script reference should be the same after renaming asset");
            Assert.IsFalse(File.Exists(k_ScriptAPath), "Old file should not exist anymore");
            Assert.IsTrue(File.Exists(k_ScriptBPath), "New file should exist on disk");
        }

        [TestMethod]
        public void Refresh_WithDeletedFiles_RemovesThemFromManager()
        {
            AssetManager.CreateAsset(new Recording(guid), k_ScriptAPath);
            AssetManager.CreateAsset(new Recording(guid), k_ScriptBPath);
            AssetManager.CreateAsset(new Recording(guid), k_ScriptCPath);

            File.Delete(k_ScriptAPath);
            File.Delete(k_ScriptCPath);
            AssetManager.Refresh();

            var assetA = AssetManager.GetAsset(k_ScriptAPath);
            var assetB = AssetManager.GetAsset(k_ScriptBPath);
            var assetC = AssetManager.GetAsset(k_ScriptCPath);

            Assert.AreEqual(1, AssetManager.Assets.Count());
            Assert.IsNull(assetA, "Asset A should not exist anymore");
            Assert.IsNotNull(assetB, "Asset B should exist");
            Assert.IsNull(assetC, "Asset C should not exist anymore");
        }

        [TestMethod]
        public void Refresh_WithNewFiles_AddsThemToManager()
        {
            AssetManager.CreateAsset(new Recording(guid), k_ScriptAPath);

            CreateDummyScriptWithImporter(k_ScriptBPath);
            CreateDummyScriptWithImporter(k_ScriptCPath);
            AssetManager.Refresh();

            var assetA = AssetManager.GetAsset(k_ScriptAPath);
            var assetB = AssetManager.GetAsset(k_ScriptBPath);
            var assetC = AssetManager.GetAsset(k_ScriptCPath);

            Assert.AreEqual(3, AssetManager.Assets.Count());
            Assert.IsNotNull(assetA, "Asset A should exist");
            Assert.IsNotNull(assetB, "Asset B should exist");
            Assert.IsNotNull(assetC, "Asset C should exist");
        }

        [TestMethod]
        public void Refresh_WithRenamedFiles_HavingOtherFilesWithSameHash_AcceptsRename()
        {
            var assetA = AssetManager.CreateAsset(new Recording(guid), k_ScriptAPath);
            var assetB = AssetManager.CreateAsset(new Recording(guid), k_ScriptBPath);
            var assetC = AssetManager.CreateAsset(new Recording(guid), k_ScriptCPath);

            File.Move(k_ScriptBPath, k_ScriptDPath);
            AssetManager.Refresh();

            Assert.AreEqual(3, AssetManager.Assets.Count());
            Assert.AreEqual(assetA.Guid, AssetManager.GetAsset(k_ScriptAPath).Guid, "A asset guid missmath");
            Assert.AreEqual(assetB.Guid, AssetManager.GetAsset(k_ScriptDPath).Guid, "B asset guid missmath");
            Assert.AreEqual(assetC.Guid, AssetManager.GetAsset(k_ScriptCPath).Guid, "C asset guid missmath");
            Assert.IsNull(AssetManager.GetAsset(k_ScriptBPath), "B asset was renamed, so should not appear");
        }

        [TestMethod]
        public void Refresh_WithRenamedFiles_AndOneBeingDeleted_AcceptsRename()
        {
            var assetA = AssetManager.CreateAsset(new Recording(guid), k_ScriptAPath);
            var assetB = AssetManager.CreateAsset(new Recording(guid), k_ScriptBPath);
            var assetC = AssetManager.CreateAsset(new Recording(guid), k_ScriptCPath);

            // This will actually think that A was renamed to D, and B deleted
            // this is due to having the same hash
            File.Move(k_ScriptBPath, k_ScriptDPath);
            File.Delete(k_ScriptAPath);
            AssetManager.Refresh();

            Assert.AreEqual(2, AssetManager.Assets.Count());
            Assert.AreEqual(assetA.Guid, AssetManager.GetAsset(k_ScriptDPath).Guid, "B asset guid missmath");
            Assert.AreEqual(assetC.Guid, AssetManager.GetAsset(k_ScriptCPath).Guid, "C asset guid missmath");
            Assert.IsNull(AssetManager.GetAsset(k_ScriptAPath), "A asset was renamed, so should not appear");
            Assert.IsNull(AssetManager.GetAsset(k_ScriptBPath), "B asset was renamed, so should not appear");
        }

        [TestMethod]
        public void DeleteAsset_RemovesFromManager_AndFromDisk()
        {
            AssetManager.CreateAsset(new Recording(guid), k_ScriptAPath);
            AssetManager.CreateAsset(new Recording(guid), k_ScriptBPath);

            AssetManager.DeleteAsset(k_ScriptAPath);

            Assert.AreEqual(1, AssetManager.Assets.Count());
            Assert.IsFalse(File.Exists(k_ScriptAPath), "Asset A should not exist on disk");
            Assert.IsNull(AssetManager.GetAsset(k_ScriptAPath), "Asset A should not exist anymore");
            Assert.IsNotNull(AssetManager.GetAsset(k_ScriptBPath), "Asset B should exist");
        }

        private static void CreateDummyScriptWithImporter(string path)
        {
            var importer = EditorAssetImporter.FromPath(path);
            importer.Value = new Recording(guid);
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
            ScriptManager = container.Resolve<IHierarchyManager>();

            ProjectManager.InitProject(TempProjectPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            DirectoryInfo di = new DirectoryInfo(TempProjectPath + "\\" + Paths.ScriptFolder);
            foreach (FileInfo file in di.GetFiles())
                file.Delete();

            AssetManager.Refresh();
        }
    }
}
