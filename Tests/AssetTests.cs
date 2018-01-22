using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Robot;
using RobotRuntime.Commands;
using Robot.Scripts;
using System.IO;

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

        [TestMethod]
        public void TwoIdenticalAssets_HaveTheSameHash_ButDifferentGuids()
        {
            var asset = AssetManager.Instance.CreateAsset(new Script(), k_ScriptAPath);
            var asset2 = AssetManager.Instance.CreateAsset(new Script(), k_ScriptBPath);

            Assert.AreEqual(asset.Hash, asset2.Hash, "Identical assets should have same hash");
            Assert.AreNotEqual(asset.Guid, asset2.Guid, "Identical assets should have different GUIDs");
        }

        [TestMethod]
        public void CreateAsset_CreatesFile_AndAddsToAssetManager()
        {
            var script = new Script();
            AssetManager.Instance.CreateAsset(script, k_ScriptAPath);

            Assert.IsTrue(File.Exists(k_ScriptAPath), "File was not created");
            Assert.AreEqual(AssetManager.Instance.GetAsset(k_ScriptAPath).Importer.Load<Script>(), script, "Asset value did not match the script");
        }

        [TestMethod]
        public void ReloadScript_UpdatesScriptRef_FromFile()
        {
            var script = ScriptManager.Instance.LoadedScripts[0];
            ScriptManager.Instance.SaveScript(script, k_ScriptAPath);
            script.AddCommand(new CommandSleep(5));

            var script2 = ScriptManager.Instance.LoadScript(k_ScriptAPath);

            Assert.AreNotEqual(script, script2, "Reloading script should give new reference type object");
            Assert.AreEqual(script2, ScriptManager.Instance.LoadedScripts[0], "Scripts from assets and in script manager should be the same");
            Assert.AreEqual(0, script2.Commands.Count(), "Script should not have commands");
        }

        [TestMethod]
        public void RenameAsset_RenamesFile_AndKeepsAllReferencesIntact()
        {
            var script = new Script();
            var asset = AssetManager.Instance.CreateAsset(script, k_ScriptAPath);

            AssetManager.Instance.RenameAsset(k_ScriptAPath, k_ScriptBPath);

            var newAsset = AssetManager.Instance.GetAsset(k_ScriptBPath);
            var newScript = newAsset.Importer.Load<Script>();

            Assert.AreEqual(asset, newAsset, "Asset reference should be the same after renaming asset");
            Assert.AreEqual(script, newScript, "Script reference should be the same after renaming asset");
            Assert.IsFalse(File.Exists(k_ScriptAPath), "Old file should not exist anymore");
            Assert.IsTrue(File.Exists(k_ScriptBPath), "New file should exist on disk");
        }

        [TestMethod]
        public void Refresh_WithFileBeingRenamedOnFileSystem_AcceptsRenameAndKeepsAllReferences()
        {
            var script = new Script();
            var asset = AssetManager.Instance.CreateAsset(script, k_ScriptAPath);

            File.Move(k_ScriptAPath, k_ScriptBPath);
            AssetManager.Instance.Refresh();

            var newAsset = AssetManager.Instance.GetAsset(k_ScriptBPath);
            var newScript = newAsset.Importer.Load<Script>();

            Assert.AreEqual(asset, newAsset, "Asset reference should be the same after renaming asset");
            Assert.AreEqual(script, newScript, "Script reference should be the same after renaming asset");
            Assert.IsFalse(File.Exists(k_ScriptAPath), "Old file should not exist anymore");
            Assert.IsTrue(File.Exists(k_ScriptBPath), "New file should exist on disk");
        }

        [TestMethod]
        public void Refresh_WithDeletedFiles_RemovesThemFromManager()
        {
            AssetManager.Instance.CreateAsset(new Script(), k_ScriptAPath);
            AssetManager.Instance.CreateAsset(new Script(), k_ScriptBPath);
            AssetManager.Instance.CreateAsset(new Script(), k_ScriptCPath);

            File.Delete(k_ScriptAPath);
            File.Delete(k_ScriptCPath);
            AssetManager.Instance.Refresh();

            var assetA = AssetManager.Instance.GetAsset(k_ScriptAPath);
            var assetB = AssetManager.Instance.GetAsset(k_ScriptBPath);
            var assetC = AssetManager.Instance.GetAsset(k_ScriptCPath);

            Assert.AreEqual(1, AssetManager.Instance.Assets.Count());
            Assert.IsNull(assetA, "Asset A should not exist anymore");
            Assert.IsNotNull(assetB, "Asset B should exist");
            Assert.IsNull(assetC, "Asset C should not exist anymore");
        }

        [TestMethod]
        public void Refresh_WithNewFiles_AddsThemToManager()
        {
            AssetManager.Instance.CreateAsset(new Script(), k_ScriptAPath);

            CreateDummyScriptWithImporter(k_ScriptBPath);
            CreateDummyScriptWithImporter(k_ScriptCPath);
            AssetManager.Instance.Refresh();

            var assetA = AssetManager.Instance.GetAsset(k_ScriptAPath);
            var assetB = AssetManager.Instance.GetAsset(k_ScriptBPath);
            var assetC = AssetManager.Instance.GetAsset(k_ScriptCPath);

            Assert.AreEqual(3, AssetManager.Instance.Assets.Count());
            Assert.IsNotNull(assetA, "Asset A should exist");
            Assert.IsNotNull(assetB, "Asset B should exist");
            Assert.IsNotNull(assetC, "Asset C should exist");
        }

        [TestMethod]
        public void Refresh_WithRenamedFiles_HavingOtherFilesWithSameHash_AcceptsRename()
        {
            var assetA = AssetManager.Instance.CreateAsset(new Script(), k_ScriptAPath);
            var assetB = AssetManager.Instance.CreateAsset(new Script(), k_ScriptBPath);
            var assetC = AssetManager.Instance.CreateAsset(new Script(), k_ScriptCPath);

            File.Move(k_ScriptBPath, k_ScriptDPath);
            AssetManager.Instance.Refresh();

            Assert.AreEqual(3, AssetManager.Instance.Assets.Count());
            Assert.AreEqual(assetA.Guid, AssetManager.Instance.GetAsset(k_ScriptAPath).Guid, "A asset guid missmath");
            Assert.AreEqual(assetB.Guid, AssetManager.Instance.GetAsset(k_ScriptDPath).Guid, "B asset guid missmath");
            Assert.AreEqual(assetC.Guid, AssetManager.Instance.GetAsset(k_ScriptCPath).Guid, "C asset guid missmath");
            Assert.IsNull(AssetManager.Instance.GetAsset(k_ScriptBPath), "B asset was renamed, so should not appear");
        }

        [TestMethod]
        public void DeleteAsset_RemovesFromManager_AndFromDisk()
        {
            AssetManager.Instance.CreateAsset(new Script(), k_ScriptAPath);
            AssetManager.Instance.CreateAsset(new Script(), k_ScriptBPath);

            AssetManager.Instance.DeleteAsset(k_ScriptAPath);

            Assert.AreEqual(1, AssetManager.Instance.Assets.Count());
            Assert.IsFalse(File.Exists(k_ScriptAPath), "Asset A should not exist on disk");
            Assert.IsNull(AssetManager.Instance.GetAsset(k_ScriptAPath), "Asset A should not exist anymore");
            Assert.IsNotNull(AssetManager.Instance.GetAsset(k_ScriptBPath), "Asset B should exist");
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
            MouseRobot.Instance.SetupProjectPath(TempProjectPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            DirectoryInfo di = new DirectoryInfo(TempProjectPath + "\\" + AssetManager.ScriptFolder);
            foreach (FileInfo file in di.GetFiles())
                file.Delete();

            AssetManager.Instance.Refresh();
        }
    }
}
