using Robot.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Robot.Assets
{
    /// <summary>
    /// Subscribes to all AssetManager callbacks and collects all modified assets in a list, fires callback after refresh.
    /// This class can be Injected as Dependency for other managers.
    /// </summary>
    public class ModifiedAssetCollector : IModifiedAssetCollector
    {
        public IList<string> ExtensionFilters { get; } = new List<string>();

        public IList<string> Paths { get; } = new List<string>();
        public event Action<IList<string>> AssetsModified;

        private bool m_FirstRefresh = true;

        private IAssetManager AssetManager;
        public ModifiedAssetCollector(IAssetManager AssetManager)
        {
            this.AssetManager = AssetManager;

            AssetManager.AssetCreated += AddPathToList;
            AssetManager.AssetUpdated += AddPathToList;
            AssetManager.AssetDeleted += AddPathToList;
            AssetManager.RefreshFinished += OnAssetRefreshFinished;
        }

        private void AddPathToList(string assetPath)
        {
            if (ExtensionFilters.Count == 0 || ExtensionFilters.Any(filter => assetPath.EndsWith(filter)))
                Paths.Add(assetPath);

            // If asset manager is not set to batch asset editing mode, that means no refresh will be called,
            // but something has already changed from within app. Call refresh callback manually.
            if (!AssetManager.IsEditingAssets)
                OnAssetRefreshFinished();
        }

        private void OnAssetRefreshFinished()
        {
            if (Paths.Count == 0 && !m_FirstRefresh)
                return;

            m_FirstRefresh = false;
            AssetsModified?.Invoke(Paths);
            Paths.Clear();
        }
    }
}
