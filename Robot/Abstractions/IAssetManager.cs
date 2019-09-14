using System;
using System.Collections.Generic;

namespace Robot.Abstractions
{
    public interface IAssetManager
    {
        IEnumerable<Asset> Assets { get; }

        /// <summary>
        /// Specifies if one should load assets or not.
        /// Usually it is false on first refresh while scripts haven't yet been compiled and loaded.
        /// Ofter scripts are compiled and loaded, this will become true.
        /// This property should be set by project manager when scripts successfully compiled.
        /// Implicit dependency is troublesome, but there is no nice way around it.
        /// </summary>
        bool CanLoadAssets { get; set; }

        /* No public refs, not needed maybe?
        Dictionary<Guid, Asset> GuidAssetTable { get; }
        Dictionary<Guid, long> GuidHashTable { get; }
        Dictionary<Guid, string> GuidPathTable { get; }
        */
        bool IsEditingAssets { get; }

        /// <summary>
        /// Called whenever Asset is created
        /// </summary>
        event Action<string> AssetCreated;

        /// <summary>
        /// Called whenever Asset is deleted
        /// </summary>
        event Action<string> AssetDeleted;

        /// <summary>
        /// Called whenever parent Asset is renamed. Nothing is called for child assets
        /// </summary>
        event Action<string, string> AssetRenamed;
        event Action<string> AssetUpdated;

        event Action RefreshFinished;
        // event Action FinishedAssetEditing; // ADD

        void BeginAssetEditing();
        void EditAssets(Action ac);
        void EndAssetEditing();

        void SaveExistngAsset(Asset existingAsset, object newValue);

        Asset CreateAsset(object assetValue, string path);
        Asset GetAsset(string path);
        void RenameAsset(string sourcePath, string destPath);
        void DeleteAsset(string path);

        void Refresh();
    }
}