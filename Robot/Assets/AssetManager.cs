using Robot.Scripts;
using RobotRuntime;
using RobotRuntime.Assets;
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
        private AssetManager() { }

        public Dictionary<Guid, Asset> GuidAssetTable { get; private set; } = new Dictionary<Guid, Asset>();
        public Dictionary<Guid, string> GuidPathTable { get; private set; } = new Dictionary<Guid, string>();        
        public Dictionary<Guid, Int64> GuidHashTable { get; private set; } = new Dictionary<Guid, Int64>();        

        public event Action RefreshFinished;
        public event Action<string, string> AssetRenamed;
        public event Action<string> AssetDeleted;
        public event Action<string> AssetCreated;
        public event Action<string> AssetUpdated;

        public const string ScriptFolder = "Scripts";
        public const string ImageFolder = "Images";

        private string ScriptPath { get { return Path.Combine(Environment.CurrentDirectory, ScriptFolder); } }
        private string ImagePath { get { return Path.Combine(Environment.CurrentDirectory, ImageFolder); } }

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
                    if (assetWithSameHash != null && !GuidPathTable.ContainsValue(assetWithSameHash.Path))
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

            AssetGuidManager.Instance.AddPathToGuid(asset.Guid, asset.Path);
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
            GuidAssetTable.Remove(asset.Guid);
            GuidPathTable.Remove(asset.Guid);
            GuidHashTable.Remove(asset.Guid);
            AssetGuidManager.Instance.RemoveGuid(asset.Guid);

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

            DeleteAssetInternal(asset, true);
            asset.UpdatePath(destPath);
            AddAssetInternal(asset, true);

            AssetGuidManager.Instance.AddPathToGuid(asset.Guid, asset.Path);
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
            GuidAssetTable.Add(asset.Guid, asset);
            GuidPathTable.Add(asset.Guid, asset.Path);
            GuidHashTable.Add(asset.Guid, asset.Hash);
            AssetGuidManager.Instance.AddPathToGuid(asset.Guid, asset.Path);

            if (!silent)
                AssetCreated?.Invoke(asset.Path);
        }

        private List<string> GetAllPathsInProjectDirectory()
        {
            var paths = new List<string>();

            foreach (string fileName in Directory.GetFiles(ImagePath, "*.png").Select(Path.GetFileName))
                paths.Add(ImageFolder + "\\" + fileName);

            foreach (string fileName in Directory.GetFiles(ScriptPath, "*.mrb").Select(Path.GetFileName))
                paths.Add(ScriptFolder + "\\" + fileName);
            return paths;
        }

        public void InitProject()
        {
            if (!Directory.Exists(ScriptPath))
                Directory.CreateDirectory(ScriptPath);

            if (!Directory.Exists(ImagePath))
                Directory.CreateDirectory(ImagePath);

            if (!Directory.Exists(AssetGuidManager.Instance.MetadataPath))
                Directory.CreateDirectory(AssetGuidManager.Instance.MetadataPath);
        }
    }
}
