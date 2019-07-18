using Robot.Abstractions;
using RobotRuntime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Robot.Assets
{
    /// <summary>
    /// Subscribes to all AssetManager callbacks and collects all modified assets in a list, fires callback after refresh.
    /// This class can be Injected as Dependency for other managers.
    /// </summary>
    [RegisterTypeToContainer(typeof(IModifiedAssetCollector))]
    public class ModifiedAssetCollector : IModifiedAssetCollector
    {
        public bool AutoClear { get; set; } = true;

        public IList<string> ExtensionFilters { get; } = new List<string>();

        public IList<string> ModifiedAssetPaths { get; } = new List<string>();
        public event Action<IEnumerable<string>> AssetsModified;

        public IList<(string From, string To)> RenamedAssetPaths { get; } = new List<(string, string)>();
        public event Action<IEnumerable<(string From, string To)>> AssetsRenamed;

        private bool m_FirstRefresh = true;

        private IAssetManager AssetManager;
        public ModifiedAssetCollector(IAssetManager AssetManager)
        {
            this.AssetManager = AssetManager;

            AssetManager.AssetCreated += OnAssetsModified;
            AssetManager.AssetUpdated += OnAssetsModified;
            AssetManager.AssetDeleted += OnAssetsModified;
            AssetManager.AssetRenamed += OnAssetsRenamed;
            AssetManager.RefreshFinished += OnAssetRefreshFinished;
        }

        private void OnAssetsRenamed(string from, string to)
        {
            if (FilterMatchesAssetPath(from) || FilterMatchesAssetPath(to))
                RenamedAssetPaths.Add((from, to));

            // If asset manager is not set to batch asset editing mode, that means no refresh will be called,
            // but something has already changed from within app. Call refresh callback manually.
            if (!AssetManager.IsEditingAssets)
                OnAssetRefreshFinished();
        }

        private void OnAssetsModified(string assetPath)
        {
            if (FilterMatchesAssetPath(assetPath))
                ModifiedAssetPaths.Add(assetPath);

            // If asset manager is not set to batch asset editing mode, that means no refresh will be called,
            // but something has already changed from within app. Call refresh callback manually.
            if (!AssetManager.IsEditingAssets)
                OnAssetRefreshFinished();
        }

        private bool FilterMatchesAssetPath(string assetPath)
        {
            return ExtensionFilters.Count == 0 || ExtensionFilters.Any(filter => assetPath.EndsWith(filter));
        }

        private void OnAssetRefreshFinished()
        {
            var shouldCallModifiedAssetCallback = ModifiedAssetPaths.Count != 0 || m_FirstRefresh;
            var shouldCallRenamedAssetCallback = RenamedAssetPaths.Count != 0 || m_FirstRefresh;

            if (shouldCallModifiedAssetCallback)
            {
                m_FirstRefresh = false;
                AssetsModified?.Invoke(ModifiedAssetPaths.ToArray());

                if (AutoClear)
                    ModifiedAssetPaths.Clear();
            }
            if (shouldCallRenamedAssetCallback)
            {
                m_FirstRefresh = false;
                AssetsRenamed?.Invoke(RenamedAssetPaths.ToArray());

                if (AutoClear)
                    RenamedAssetPaths.Clear();
            }
        }

        public void Clear()
        {
            ModifiedAssetPaths.Clear();
            RenamedAssetPaths.Clear();
        }
    }
}
