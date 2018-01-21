using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Robot;
using RobotRuntime.Commands;
using Robot.Scripts;
using System.IO;
using RobotRuntime.Assets;

namespace Tests
{
    [TestClass]
    public class AssetGuidManagerTests
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

        [TestMethod]
        public void NewAssets_UponRefresh_AreAddedToGuidTable()
        {
            CreateDummyScriptWithImporter(k_ScriptAPath);
            CreateDummyScriptWithImporter(k_ScriptBPath);

            AssetManager.Instance.Refresh();

            Assert.AreEqual(2, AssetGuidManager.Instance.GetEnumarable().Count(), "Asset count missmatch");
            Assert.AreEqual(AssetManager.Instance.GetAsset(k_ScriptAPath).Guid, AssetGuidManager.Instance.GetGuid(k_ScriptAPath), "Asset guid missmatch");
            Assert.AreEqual(AssetManager.Instance.GetAsset(k_ScriptBPath).Guid, AssetGuidManager.Instance.GetGuid(k_ScriptBPath), "Asset guid missmatch");
        }
        /*
        [TestMethod]
        public void NewAssets_UponRefresh_AreAddedToGuidTable()
        {
            CreateDummyScriptWithImporter
            var asset = AssetManager.Instance.CreateAsset(new Script(), k_ScriptAPath);
            var asset2 = AssetManager.Instance.CreateAsset(new Script(), k_ScriptBPath);

            Assert.AreEqual(asset.Hash, asset2.Hash, "Identical assets should have same hash");
            Assert.AreNotEqual(asset.Guid, asset2.Guid, "Identical assets should have different GUIDs");
        }*/

        private static void CreateDummyScriptWithImporter(string path)
        {
            var importer = EditorAssetImporter.FromPath(path);
            importer.Value = new Script();
            importer.SaveAsset();
        }

        [TestInitialize]
        public void Initialize()
        {
            Cleanup();
            MouseRobot.Instance.SetupProjectPath(TempProjectPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(TempProjectPath + "\\" + AssetManager.ScriptFolder))
            {
                DirectoryInfo di = new DirectoryInfo(TempProjectPath + "\\" + AssetManager.ScriptFolder);
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
            }

            if (Directory.Exists(TempProjectPath + "\\" + AssetGuidManager.MetadataFolder))
            {
                DirectoryInfo di = new DirectoryInfo(TempProjectPath + "\\" + AssetGuidManager.MetadataFolder);
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
            }
            AssetGuidManager.Instance.LoadMetaFiles();
        }
    }
}
