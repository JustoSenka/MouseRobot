using Robot.Abstractions;
using Robot.Assets.Abstractions;
using RobotRuntime;
using RobotRuntime.Abstractions;
using RobotRuntime.Assets;
using RobotRuntime.Logging;
using RobotRuntime.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Lifetime;

namespace Robot
{
    [RegisterTypeToContainer(typeof(IAssetManager), typeof(ContainerControlledLifetimeManager))]
    [RegisterTypeToContainer(typeof(IRuntimeAssetManager), typeof(ContainerControlledLifetimeManager))]
    public class AssetManager : RuntimeAssetManager, IAssetManager, IRuntimeAssetManager
    {
        private Dictionary<Guid, Asset> GuidAssetTable { get; set; } = new Dictionary<Guid, Asset>();
        private Dictionary<Guid, string> GuidPathTable { get; set; } = new Dictionary<Guid, string>();
        private Dictionary<Guid, Int64> GuidHashTable { get; set; } = new Dictionary<Guid, Int64>();

        public event Action RefreshFinished;
        public event Action<string, string> AssetRenamed;
        public event Action<string> AssetDeleted;
        public event Action<string> AssetCreated;
        public event Action<string> AssetUpdated;

        public bool IsEditingAssets { get; private set; }

        public IEnumerable<Asset> Assets => GuidAssetTable.Select(pair => pair.Value);

        private DateTime m_LastRefresh = DateTime.MinValue;

        private readonly IAssetGuidManager AssetGuidManager;
        private readonly IProfiler Profiler;
        private readonly IStatusManager StatusManager;
        private readonly IAssetManagerStartupAnalytics AssetManagerStartupAnalytics;
        public AssetManager(IAssetGuidManager AssetGuidManager, IProfiler Profiler, IStatusManager StatusManager, ILogger Logger,
            IAssetManagerStartupAnalytics AssetManagerStartupAnalytics) :
            base(AssetGuidManager, Logger, Profiler)
        {
            this.AssetGuidManager = AssetGuidManager;
            this.Profiler = Profiler;
            this.StatusManager = StatusManager;
            this.AssetManagerStartupAnalytics = AssetManagerStartupAnalytics;
        }

        public void Refresh()
        {
            Profiler.Start("AssetManager_Refresh");
            BeginAssetEditing();

            // Get all modified asset paths
            var allAssetPaths = Paths.GetAllAssetPaths(true);
            var modifiedAssetPaths = allAssetPaths.Where(p =>
            {
                // Return all directories
                if (Paths.IsDirectory(p))
                    return true;

                var fileInfo = new FileInfo(p);
                if (p == null)
                    return true;

                // Return path if asset is a newly added one and doesn't exist in memory yet
                if (!GuidPathTable.ContainsValue(p))
                    return true;

                // Return only those files which were written to recently
                return fileInfo.LastWriteTime > m_LastRefresh || fileInfo.LastWriteTime > m_LastRefresh;
            });

            var allAssetsOnDisk = allAssetPaths.Select(path => new Asset(path)).ToArray();
            var modifiedAssetsOnDisk = modifiedAssetPaths.Select(path => new Asset(path, true)).ToArray();

            // Add all newly created folders, so UI will be able process callbacks about renaming correctly
            // Since rename logic is in the middle and it has to be in the middle, while creation is in the end, callbacks used to fail
            // Rename logic must come before asset creation logic, because it is unknown whether new asset was created or old renamed asset
            // And it is unknown what GUID to give that asset at that point
            foreach (var assetOnDisk in modifiedAssetsOnDisk)
            {
                if (Paths.IsDirectory(assetOnDisk.Path) && !GuidPathTable.ContainsValue(assetOnDisk.Path))
                    AddAssetInternal(assetOnDisk);
            }

            // Detect renamed assets if application was closed, and assets were renamed via file system
            foreach (var pair in AssetGuidManager.Paths.ToList())
            {
                var path = pair.Value;
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    var guid = pair.Key;
                    var hash = AssetGuidManager.GetHash(guid);

                    var assetOnDiskWithSameHashButNotKnownPath = modifiedAssetsOnDisk.FirstOrDefault(
                        a => a.Hash == hash && !AssetGuidManager.ContainsValue(a.Path));

                    // If this asset on disk is found, update pathin guid map on AssetGuidManager, since best prediction is that it was renamed
                    // Only AssetGuidManager is updated, AssetManager will get updated in following foreach statement
                    if (assetOnDiskWithSameHashButNotKnownPath != null)
                    {
                        AssetGuidManager.AddNewGuid(guid, assetOnDiskWithSameHashButNotKnownPath.Path, hash);
                        Logger.Log(LogType.Log, "Asset '" + assetOnDiskWithSameHashButNotKnownPath.Name + "' was recognized as renamed asset");
                    }
                }
            }

            // Detect Rename for assets in memory (while keeping existing asset references)
            foreach (var assetInMemory in Assets.ToArray()) // making a copy of list since we're modifying the collection
            {
                if (!File.Exists(assetInMemory.Path) && !Directory.Exists(assetInMemory.Path))
                {
                    // if known path does not exist on disk anymore but some other asset with same hash exists on disk, it must have been renamed
                    var assetWithSameHashAndNotInDbYet = modifiedAssetsOnDisk.FirstOrDefault(asset =>
                    asset.Hash == assetInMemory.Hash && !GuidPathTable.ContainsValue(asset.Path));

                    // If asset with same hash exists, asset was renamed. If not, deleted or unmodified asset.
                    // Not deleting asset here since we are only comparing to modifiedAssetsOnDisk
                    if (assetWithSameHashAndNotInDbYet != null)
                    {
                        RenameAssetInternal(assetInMemory.Path, assetWithSameHashAndNotInDbYet.Path);
                        Logger.Log(LogType.Log, "Asset '" + assetInMemory.Name + "' was renamed to '" + assetWithSameHashAndNotInDbYet.Name + "'");
                    }
                }
            }

            // Detect deleted assets
            foreach (var assetInMemory in Assets.ToArray()) // making a copy of list since we're modifying the collection
            {
                if (!File.Exists(assetInMemory.Path) && !Directory.Exists(assetInMemory.Path))
                {
                    // If file does not exist anymore, it might have been deleted or renamed.
                    // All RENAME operations should have already been handled by foreach statement above
                    // Thus only deleted assets remain
                    DeleteAssetInternal(assetInMemory);
                    Logger.Log(LogType.Log, "Asset was deleted: '" + assetInMemory.Name + "'");
                }
            }

            // Add new assets and detect modifications
            foreach (var assetOnDisk in modifiedAssetsOnDisk)
            {
                var isHashKnown = GuidHashTable.ContainsValue(assetOnDisk.Hash);
                var isPathKnown = GuidPathTable.ContainsValue(assetOnDisk.Path);

                // We know the path, but hash has changed, must have been modified
                if (!isHashKnown && isPathKnown)
                {
                    UpdateAssetInternal(GetAsset(assetOnDisk.Path));
                    Logger.Log(LogType.Log, "Asset was modified: '" + assetOnDisk.Name + "'");
                }
                // New file added
                else if (!isPathKnown)
                {
                    AddAssetInternal(assetOnDisk);
                }
            }

            Profiler.Stop("AssetManager_Refresh");

            StatusManager.Add("AssetManager", 10, new Status(null, "Asset Refresh Finished", StandardColors.Default));
            Logger.Log(LogType.Debug, "Asset refresh finished");

            // Making copy, so it could be iterated by any thread
            if (m_LastRefresh == DateTime.MinValue)
                AssetManagerStartupAnalytics.CountAndReportAssetTypes(Assets.ToArray());

            m_LastRefresh = DateTime.Now;

            EndAssetEditing();
        }

        public void SaveExistngAsset(Asset existingAsset, object newValue)
        {
            existingAsset.Value = newValue;
            existingAsset.SaveAsset();
            existingAsset.Update();
            AssetUpdated?.Invoke(existingAsset.Path);

            AssetGuidManager.AddNewGuid(existingAsset.Guid, existingAsset.Path, existingAsset.Hash);
            if (!IsEditingAssets)
                AssetGuidManager.Save();
        }

        /// <summary>
        /// Asset path must be with an extension, if not, it will not know how to save the asset.
        /// If asset path is without extension, folder at path will be created
        /// </summary>
        public Asset CreateAsset(object assetValue, string path)
        {
            path = Paths.GetRelativePath(path).NormalizePath();

            if (path.IsEmpty())
            {
                Logger.Log(LogType.Error, "Cannot save asset. Given path is invalid: " + path);
                return null;
            }

            var asset = GetAsset(path);
            if (asset != null)
            {
                asset.Value = assetValue;
                asset.SaveAsset();
                asset.Update();
                AssetUpdated?.Invoke(path);
            }
            else
            {
                var dirParent = Paths.GetPathParent(path);
                if (!Directory.Exists(dirParent))
                {
                    Logger.Log(LogType.Error, "Cannot create asset if parent directory does not exists: " + dirParent);
                    return null;
                }

                asset = new Asset(path, true);
                asset.Value = assetValue;
                asset.SaveAsset();
                asset.Update();
                AddAssetInternal(asset);
            }

            AssetGuidManager.AddNewGuid(asset.Guid, asset.Path, asset.Hash);
            if (!IsEditingAssets)
                AssetGuidManager.Save();

            return asset;
        }

        /// <summary>
        /// Removes asset from memory and deletes its corresponding file from disk
        /// </summary>
        public void DeleteAsset(string path)
        {
            path = Paths.GetRelativePath(path).NormalizePath();
            if (path.IsEmpty())
            {
                Logger.Log(LogType.Error, "Tried to delete empty directory. This might lead to really bad outcome!");
                return;
            }

            var asset = GetAsset(path);
            if (asset == null)
            {
                Logger.Log(LogType.Error, "Cannot detele file or directory because Asset Manager does not know about this path." +
                    " Not deleting anything for safety reasons: " + path);
                return;
            }

            var isDirectory = Directory.Exists(path);

            var wasEditingAssets = IsEditingAssets;
            try
            {
                if (isDirectory)
                {
                    IsEditingAssets = true;
                    Directory.Delete(path, true);
                }
                else
                {
                    File.SetAttributes(path, FileAttributes.Normal);
                    File.Delete(path);
                }
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Cannot delete asset at path: " + path, e.Message);
            }

            if (isDirectory)
            {
                var pathsInsideTheDir = path + Path.DirectorySeparatorChar;
                foreach (var assetInDir in Assets.Where(a => a.Path.StartsWith(pathsInsideTheDir)).ToArray())
                {
                    // delete all assets except the original directory asset
                    DeleteAssetInternal(assetInDir);
                }

                IsEditingAssets = wasEditingAssets;
            }

            DeleteAssetInternal(asset);
        }

        private void DeleteAssetInternal(Asset asset, bool silent = false)
        {
            GuidAssetTable.Remove(asset.Guid);
            GuidPathTable.Remove(asset.Guid);
            GuidHashTable.Remove(asset.Guid);

            if (!silent)
                AssetDeleted?.Invoke(asset.Path);
        }

        /// <summary>
        /// Renames asset from memory and renames its corresponding file. Asset will keep the same guid and GuidMap will be updated.
        /// If directory is renamed, all assets inside will update their path
        /// Whole file path can be renamed, which will result into moving it to different directory
        /// If folder is renamed, only one callback is called for that specific folder
        /// </summary>
        public void RenameAsset(string sourcePath, string destPath)
        {
            if (sourcePath == destPath)
            {
                Logger.Log(LogType.Warning, "Tried to rename asset but source file name and destination are the same: " + sourcePath);
                return;
            }

            sourcePath = Paths.GetRelativePath(sourcePath).NormalizePath();
            destPath = Paths.GetRelativePath(destPath).NormalizePath();

            var asset = GetAsset(sourcePath);
            if (asset == null)
            {
                Logger.Log(LogType.Error, "Cannot rename asset because it is not known by asset manager.");
                return;
            }

            var destAsset = GetAsset(destPath);
            if (destAsset != null)
            {
                Logger.Log(LogType.Error, "Cannot rename asset destination path already exist.");
                return;
            }

            var isDirectory = Paths.IsDirectory(sourcePath);
            if (isDirectory)
            {
                var wasEditingAssets = IsEditingAssets;
                IsEditingAssets = true;

                if (destPath.IsSubDirectoryOf(sourcePath))
                {
                    Logger.Log(LogType.Warning, "Folder cannot be moved inside itself: " + sourcePath);
                    return;
                }

                Directory.Move(sourcePath, destPath);

                // Renames directory and all assets inside
                foreach (var assetInDir in Assets.Where(a => a.Path.IsSubDirectoryOf(sourcePath)).Select(a => a.Path).ToArray())
                {
                    if (assetInDir != sourcePath) // Rename all assets except the folder
                        RenameAssetInternal(assetInDir, assetInDir.Replace(sourcePath, destPath));
                }

                IsEditingAssets = wasEditingAssets;
            }
            else
            {
                File.SetAttributes(sourcePath, FileAttributes.Normal);
                File.Move(sourcePath, destPath);
            }

            // Rename in db the actual file or folder
            RenameAssetInternal(sourcePath, destPath);
        }

        private void RenameAssetInternal(string sourcePath, string destPath, bool silent = false)
        {
            var asset = GetAsset(sourcePath);
            var guid = asset.Guid;

            DeleteAssetInternal(asset, true);
            asset.UpdatePath(destPath);

            // AddAssetInternal deals with it
            // AssetGuidManager.AddNewGuid(guid, asset.Path, asset.Hash);
            if (!IsEditingAssets)
                AssetGuidManager.Save();

            AddAssetInternal(asset, true);

            if (!silent)
                AssetRenamed?.Invoke(sourcePath, destPath);
        }

        public Asset GetAsset(string path)
        {
            path = Paths.GetRelativePath(path);
            return Assets.FirstOrDefault((a) => Paths.AreRelativePathsEqual(a.Path, path));
        }

        public void BeginAssetEditing()
        {
            IsEditingAssets = true;
        }

        public void EndAssetEditing()
        {
            IsEditingAssets = false;
            AssetGuidManager.Save();
            RefreshFinished?.Invoke();
        }

        public void EditAssets(Action ac)
        {
            BeginAssetEditing();
            ac.Invoke();
            EndAssetEditing();
        }

        private void AddAssetInternal(Asset asset, bool silent = false)
        {
            var guid = AssetGuidManager.GetGuid(asset.Path);
            if (guid != default(Guid))
                asset.SetGuid(guid);

            GuidAssetTable.Add(asset.Guid, asset);
            GuidPathTable.Add(asset.Guid, asset.Path);
            GuidHashTable.Add(asset.Guid, asset.Hash);

            AssetGuidManager.AddNewGuid(asset.Guid, asset.Path, asset.Hash);
            if (!IsEditingAssets)
                AssetGuidManager.Save();

            if (!silent)
                AssetCreated?.Invoke(asset.Path);
        }

        /// <summary>
        /// This should be called whenever asset value was modified on disk. Updates hashes and asset value while keeping the reference
        /// </summary>
        private void UpdateAssetInternal(Asset oldAsset)
        {
            oldAsset.UpdateValueFromDisk();

            GuidHashTable[oldAsset.Guid] = oldAsset.Hash;
            AssetGuidManager.AddNewGuid(oldAsset.Guid, oldAsset.Path, oldAsset.Hash);

            AssetUpdated?.Invoke(oldAsset.Path);
        }
    }
}
