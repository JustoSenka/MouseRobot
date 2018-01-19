using Robot.Scripts;
using RobotRuntime;
using RobotRuntime.Perf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Robot
{
    public class AssetManager
    {
        static private AssetManager m_Instance = new AssetManager();
        static public AssetManager Instance { get { return m_Instance; } }
        private AssetManager()
        {
            k_ScriptPath = Path.Combine(MouseRobot.Instance.ProjectPath, ScriptFolder);
            k_ImagePath = Path.Combine(MouseRobot.Instance.ProjectPath, ImageFolder);

            InitProject();
        }

        public Dictionary<AssetGUID, Asset> GuidAssetTable { get; private set; } = new Dictionary<AssetGUID, Asset>();
        public Dictionary<AssetGUID, string> GuidPathTable { get; private set; } = new Dictionary<AssetGUID, string>();        
        public Dictionary<AssetGUID, Int64> GuidHashTable { get; private set; } = new Dictionary<AssetGUID, Int64>();        

        public event Action RefreshFinished;
        public event Action<string, string> AssetRenamed;
        public event Action<string> AssetDeleted;
        public event Action<string> AssetCreated;
        public event Action<string> AssetUpdated;

        public const string ScriptFolder = "Scripts";
        public const string ImageFolder = "Images";

        private readonly string k_ScriptPath;
        private readonly string k_ImagePath;

        public void Refresh()
        {
            Profiler.Start("AssetManager_Refresh");

            var paths = GetAllPathsInProjectDirectory();
            var assetsOnDisk = paths.Select(path => new Asset(path));

            // Detect Rename
            foreach(var assetInMemory in Assets)
            {
                if (!File.Exists(assetInMemory.Path))
                {
                    // if known path does not exist on disk anymore but some other asset with same hash exists on disk, it must have been renamed
                    var assetWithSameHash = assetsOnDisk.FirstOrDefault(asset => asset.Hash == assetInMemory.Hash);
                    if (assetWithSameHash != null && !GuidAssetTable.ContainsKey(assetWithSameHash.GUID))
                        RenameAssetInternal(assetInMemory.Path, assetWithSameHash.Path);
                    else
                        DeleteAssetInternal(assetInMemory);
                }
            }

            // Add new assets and detect modifications
            foreach (var assetOnDisk in assetsOnDisk)
            {
                var isHashKnown = GuidHashTable.ContainsValue(assetOnDisk.Hash);
                var isPathKnown = GuidPathTable.ContainsValue(assetOnDisk.Path); 

                // We know the path, but hash has changed, must have been modified
                if (!isHashKnown && isPathKnown)
                {
                    GetAsset(assetOnDisk.Path).UpdateValueFromDisk();
                    AssetUpdated?.Invoke(assetOnDisk.Path);
                }
                // New file added
                else if (!isPathKnown)
                {
                    AddAssetInternal(assetOnDisk);
                }
            }

            Profiler.Stop("AssetManager_Refresh");

            RefreshFinished?.Invoke();
        }

        public Asset CreateAsset(object assetValue, string path)
        {
            path = Commons.GetProjectRelativePath(path);
            var asset = GetAsset(path);
            if (asset != null)
            {
                asset.Importer.Value = assetValue;
                asset.Importer.SaveAsset();
                asset.Update();
                AssetUpdated?.Invoke(path);
            }
            else
            {
                asset = new Asset(path);
                asset.Importer.Value = assetValue;
                asset.Importer.SaveAsset();
                asset.Update();
                AddAssetInternal(asset);
            }
            return asset;
        }

        /// <summary>
        /// Removes asset from memory and deletes its corresponding file from disk
        /// </summary>
        public void DeleteAsset(string path)
        {
            path = Commons.GetProjectRelativePath(path);
            var asset = GetAsset(path);

            File.SetAttributes(path, FileAttributes.Normal);
            File.Delete(path);

            DeleteAssetInternal(asset);
        }

        private void DeleteAssetInternal(Asset asset, bool silent = false)
        {
            GuidAssetTable.Remove(asset.GUID);
            GuidPathTable.Remove(asset.GUID);
            GuidHashTable.Remove(asset.GUID);

            if (!silent)
                AssetDeleted?.Invoke(asset.Path);
        }

        /// <summary>
        /// Renames asset from memory and renames its corresponding file. Also will update all script references to that asset
        /// </summary>
        public void RenameAsset(string sourcePath, string destPath)
        {
            var asset = GetAsset(sourcePath);

            File.SetAttributes(sourcePath, FileAttributes.Normal);
            File.Move(sourcePath, destPath);

            RenameAssetInternal(sourcePath, destPath);
        }

        private void RenameAssetInternal(string sourcePath, string destPath)
        {
            var asset = GetAsset(sourcePath);
            var value = asset.Importer.Value;
            var oldGuid = asset.GUID; // might be useful for ref in scripts

            DeleteAssetInternal(asset, true);
            asset.UpdatePath(destPath);
            AddAssetInternal(asset, true);

            RenameAssetReferencesInAllScripts(asset.Path, destPath);
            AssetRenamed?.Invoke(sourcePath, destPath);
        }

        public Asset GetAsset(string path)
        {
            path = Commons.GetProjectRelativePath(path);
            return Assets.FirstOrDefault((a) => Commons.AreRelativePathsEqual(a.Path, path));
        }

        public Asset GetAsset(string folder, string name)
        {
            var path = folder + "\\" + name + "." + ExtensionFromFolder(folder);
            return GetAsset(path);
        }

        public IEnumerable<Asset> Assets
        {
            get
            {
                return GuidAssetTable.Select(pair => pair.Value).ToList(); // Converting to list so foreach could remove elements from hashtables while iterating
            }
        }

        public static string ExtensionFromFolder(string folder)
        {
            switch (folder)
            {
                case ScriptFolder:
                    return FileExtensions.Script;
                case ImageFolder:
                    return FileExtensions.Image;
                default:
                    return "";
            }
        }

        public static string FolderFromExtension(string path)
        {
            if (path.EndsWith(FileExtensions.Script))
                return ScriptFolder;
            else if (path.EndsWith(FileExtensions.Image))
                return ImageFolder;
            else if (path.EndsWith(FileExtensions.Timeline))
                return "Timeline";
            return "";
        }

        private void AddAssetInternal(Asset asset, bool silent = false)
        {
            GuidAssetTable.Add(asset.GUID, asset);
            GuidPathTable.Add(asset.GUID, asset.Path);
            GuidHashTable.Add(asset.GUID, asset.Hash);

            if (!silent)
                AssetCreated?.Invoke(asset.Path);
        }

        private void RenameAssetReferencesInAllScripts(string path, string destPath)
        {
            var scripts = Assets.Where(a => a.Importer.HoldsType() == typeof(Script));
        }

        private List<string> GetAllPathsInProjectDirectory()
        {
            var paths = new List<string>();

            foreach (string fileName in Directory.GetFiles(k_ImagePath, "*.png").Select(Path.GetFileName))
                paths.Add(ImageFolder + "\\" + fileName);

            foreach (string fileName in Directory.GetFiles(k_ScriptPath, "*.mrb").Select(Path.GetFileName))
                paths.Add(ScriptFolder + "\\" + fileName);
            return paths;
        }

        private void InitProject()
        {
            if (!Directory.Exists(k_ScriptPath))
                Directory.CreateDirectory(k_ScriptPath);

            if (!Directory.Exists(k_ImagePath))
                Directory.CreateDirectory(k_ImagePath);
        }
    }
}
