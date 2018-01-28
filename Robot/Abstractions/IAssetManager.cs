using System;
using System.Collections.Generic;

namespace Robot.Abstractions
{
    public interface IAssetManager
    {
        string ImageFolder { get; }
        string ScriptFolder { get; }

        IEnumerable<Asset> Assets { get; }
        Dictionary<Guid, Asset> GuidAssetTable { get; }
        Dictionary<Guid, long> GuidHashTable { get; }
        Dictionary<Guid, string> GuidPathTable { get; }

        event Action<string> AssetCreated;
        event Action<string> AssetDeleted;
        event Action<string, string> AssetRenamed;
        event Action<string> AssetUpdated;
        event Action RefreshFinished;

        void BeginAssetEditing();
        Asset CreateAsset(object assetValue, string path);
        void DeleteAsset(string path);
        void EditAssets(Action ac);
        void EndAssetEditing();
        Asset GetAsset(string path);
        Asset GetAsset(string folder, string name);
        void InitProject();
        void Refresh();
        void RenameAsset(string sourcePath, string destPath);
    }
}