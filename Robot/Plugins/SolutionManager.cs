using Robot.Abstractions;
using RobotRuntime;
using System;
using System.Collections.Generic;

namespace Robot.Plugins
{
    public class SolutionManager
    {
        private IAssetManager AssetManager;
        private IModifiedAssetCollector ModifiedAssetCollector;
        public SolutionManager(IAssetManager AssetManager, IModifiedAssetCollector ModifiedAssetCollector)
        {
            this.AssetManager = AssetManager;
            this.ModifiedAssetCollector = ModifiedAssetCollector;

            ModifiedAssetCollector.ExtensionFilters.Add(FileExtensions.PluginD);
            ModifiedAssetCollector.AssetsModified += OnAssetsModified;
        }

        private void OnAssetsModified(IList<string> modifiedAssets)
        {
            GenerateNewSolution();
        }

        public void GenerateNewSolution()
        {
            throw new NotImplementedException();
        }
    }
}
