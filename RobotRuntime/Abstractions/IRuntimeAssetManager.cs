using System;
using System.Collections.Generic;

namespace RobotRuntime.Abstractions
{
    public interface IRuntimeAssetManager
    {
        void CollectAllImporters();
        void PreloadAllAssets();

        AssetImporter GetImporterForAsset(Guid guid);

        object GetAsset(Guid guid);
        T GetAsset<T>(Guid guid) where T : class;

        IEnumerable<AssetImporter> AssetImporters { get; }
    }
}